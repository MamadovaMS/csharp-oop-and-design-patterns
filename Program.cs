using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Unity;
using Npgsql;
using Dapper;
using Serilog;

namespace Lab4_Polyclinic
{
    // ==========================================
    // 1. ПЕРЕЧИСЛЕНИЯ (Enums)
    // ==========================================
    public enum DoctorSpecialty { None = 0, Therapist = 1, Surgeon = 2, Dentist = 3 }
    
    [Flags] // Флаговое перечисление!
    public enum ServiceType { None = 0, Diagnostics = 1, Treatment = 2, Surgery = 4, Consultation = 8 }

    // ==========================================
    // 2. МОДЕЛИ (Entities)
    // ==========================================
    public class Patient
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class Doctor
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DoctorSpecialty Specialty { get; set; }
    }

    public class MedicalService
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ServiceType Type { get; set; }
    }

    public class PlanService
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
    }

    public class TreatmentPlan
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<PlanService> Services { get; set; } = new List<PlanService>();
    }

    // ==========================================
    // 3. ИНТЕРФЕЙСЫ РЕПОЗИТОРИЕВ
    // ==========================================
    public interface IPatientRepository
    {
        IEnumerable<Patient> ReadAll();
        void Create(Patient patient);
    }

    public interface IDoctorRepository
    {
        IEnumerable<Doctor> ReadAll();
        void Create(Doctor doctor);
    }

    public interface IMedicalServiceRepository
    {
        IEnumerable<MedicalService> ReadAll();
        void Create(MedicalService service);
    }

    public interface ITreatmentPlanRepository
    {
        IEnumerable<TreatmentPlan> ReadAll();
        void Create(TreatmentPlan plan);
    }

    // ==========================================
    // 4. РЕАЛИЗАЦИИ РЕПОЗИТОРИЕВ (Dapper + Serilog)
    // ==========================================
    public class BaseRepository
    {
        // СТРОКА ПОДКЛЮЧЕНИЯ (без пароля, как ты и делала)
        protected readonly string _connectionString = "Host=localhost;Port=5432;Database=polyclinic_db;Username=postgres;";
    }

    public class PatientRepository : BaseRepository, IPatientRepository
    {
        public IEnumerable<Patient> ReadAll()
        {
            Log.Information("Запрос пациентов");
            using var connection = new NpgsqlConnection(_connectionString);
            return connection.Query<Patient>("SELECT Id, FullName FROM Patients");
        }

        public void Create(Patient patient)
        {
            Log.Information("Добавление пациента: {FullName}", patient.FullName);
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Execute("INSERT INTO Patients (FullName) VALUES (@FullName)", patient);
        }
    }

    public class DoctorRepository : BaseRepository, IDoctorRepository
    {
        public IEnumerable<Doctor> ReadAll()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return connection.Query<Doctor>("SELECT Id, FullName, Specialty FROM Doctors");
        }

        public void Create(Doctor doctor)
        {
            Log.Information("Добавление врача: {FullName}", doctor.FullName);
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Execute("INSERT INTO Doctors (FullName, Specialty) VALUES (@FullName, @Specialty)", doctor);
        }
    }

    public class MedicalServiceRepository : BaseRepository, IMedicalServiceRepository
    {
        public IEnumerable<MedicalService> ReadAll()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return connection.Query<MedicalService>("SELECT Id, Name, Type FROM MedicalServices");
        }

        public void Create(MedicalService service)
        {
            Log.Information("Добавление услуги: {Name}", service.Name);
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Execute("INSERT INTO MedicalServices (Name, Type) VALUES (@Name, @Type)", service);
        }
    }

    public class TreatmentPlanRepository : BaseRepository, ITreatmentPlanRepository
    {
        public IEnumerable<TreatmentPlan> ReadAll()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return connection.Query<TreatmentPlan>("SELECT Id, PatientId, DoctorId, DateCreated FROM TreatmentPlans");
        }

        public void Create(TreatmentPlan plan)
        {
            Log.Information("Создание плана лечения для пациента {PatientId}", plan.PatientId);
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // ТРАНЗАКЦИЯ (Требование ЛР5 для связи многие-ко-многим)
            using var transaction = connection.BeginTransaction();
            try
            {
                var sqlPlan = @"INSERT INTO TreatmentPlans (PatientId, DoctorId, DateCreated) 
                                VALUES (@PatientId, @DoctorId, @DateCreated) RETURNING Id;";
                
                // Получаем ID созданного плана
                var planId = connection.ExecuteScalar<int>(sqlPlan, plan, transaction);

                var sqlService = @"INSERT INTO PlanServices (PlanId, ServiceId, Quantity) 
                                   VALUES (@PlanId, @ServiceId, @Quantity)";

                foreach (var s in plan.Services)
                {
                    connection.Execute(sqlService, new { PlanId = planId, ServiceId = s.ServiceId, Quantity = s.Quantity }, transaction);
                }

                transaction.Commit(); // Если всё ок - сохраняем
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Если ошибка - откатываем всё
                Log.Error(ex, "Ошибка при создании плана лечения (транзакция отменена)");
                throw;
            }
        }
    }

    // ==========================================
    // 5. ЗАПУСК И IoC-КОНТЕЙНЕР
    // ==========================================
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var container = new UnityContainer();
            // Регистрируем ВСЕ зависимости
            container.RegisterType<IPatientRepository, PatientRepository>();
            container.RegisterType<IDoctorRepository, DoctorRepository>();
            container.RegisterType<IMedicalServiceRepository, MedicalServiceRepository>();
            container.RegisterType<ITreatmentPlanRepository, TreatmentPlanRepository>();

            Application.Run(container.Resolve<Form1>());
        }
    }
}