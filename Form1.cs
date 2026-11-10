using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Lab4_Polyclinic
{
    public partial class Form1 : Form
    {
        private readonly IPatientRepository _patientRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IMedicalServiceRepository _serviceRepo;
        private readonly ITreatmentPlanRepository _planRepo;

        private TabControl tabControl = null!;

        private DataGridView gridPatients = null!;
        private TextBox txtPatientName = null!;

        private DataGridView gridDoctors = null!;
        private TextBox txtDoctorName = null!;
        private ComboBox cmbSpecialty = null!;

        private DataGridView gridServices = null!;
        private TextBox txtServiceName = null!;
        private CheckedListBox clbServiceType = null!;

        private DataGridView gridPlans = null!;
        private ComboBox cmbPlanPatient = null!, cmbPlanDoctor = null!, cmbPlanService = null!;
        private NumericUpDown numPlanQuantity = null!;
        private List<PlanService> currentPlanServices = new List<PlanService>();

        public Form1(IPatientRepository patientRepo, IDoctorRepository doctorRepo, IMedicalServiceRepository serviceRepo, ITreatmentPlanRepository planRepo)
        {
            _patientRepo = patientRepo;
            _doctorRepo = doctorRepo;
            _serviceRepo = serviceRepo;
            _planRepo = planRepo;

            this.Text = "Поликлиника (ЛР4-5)";
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl { Dock = DockStyle.Fill };
            this.Controls.Add(tabControl);

            CreatePatientsTab();
            CreateDoctorsTab();
            CreateServicesTab();
            CreatePlansTab();

            LoadAllData();
        }

        private void LoadAllData()
        {
            try
            {
                gridPatients.DataSource = null;
                gridPatients.DataSource = _patientRepo.ReadAll().ToList();

                gridDoctors.DataSource = null;
                gridDoctors.DataSource = _doctorRepo.ReadAll().ToList();

                gridServices.DataSource = null;
                gridServices.DataSource = _serviceRepo.ReadAll().ToList();

                gridPlans.DataSource = null;
                gridPlans.DataSource = _planRepo.ReadAll().ToList();

                cmbPlanPatient.DataSource = _patientRepo.ReadAll().ToList();
                cmbPlanPatient.DisplayMember = "FullName";
                cmbPlanPatient.ValueMember = "Id";

                cmbPlanDoctor.DataSource = _doctorRepo.ReadAll().ToList();
                cmbPlanDoctor.DisplayMember = "FullName";
                cmbPlanDoctor.ValueMember = "Id";

                cmbPlanService.DataSource = _serviceRepo.ReadAll().ToList();
                cmbPlanService.DisplayMember = "Name";
                cmbPlanService.ValueMember = "Id";
            }
            catch (Exception)
            {
                // Игнорируем ошибки при загрузке, если БД пустая
            }
        }

        private void CreatePatientsTab()
        {
            TabPage page = new TabPage("Пациенты");
            gridPatients = new DataGridView { Location = new Point(10, 10), Size = new Size(500, 400), AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            txtPatientName = new TextBox { Location = new Point(520, 10), Width = 200, PlaceholderText = "ФИО" };
            Button btnAdd = new Button { Text = "Добавить", Location = new Point(520, 40), Width = 200 };
            
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtPatientName.Text)) return;
                _patientRepo.Create(new Patient { FullName = txtPatientName.Text });
                txtPatientName.Clear();
                LoadAllData();
            };

            page.Controls.AddRange(new Control[] { gridPatients, txtPatientName, btnAdd });
            tabControl.TabPages.Add(page);
        }

        private void CreateDoctorsTab()
        {
            TabPage page = new TabPage("Врачи");
            gridDoctors = new DataGridView { Location = new Point(10, 10), Size = new Size(500, 400), AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            txtDoctorName = new TextBox { Location = new Point(520, 10), Width = 200, PlaceholderText = "ФИО Врача" };
            cmbSpecialty = new ComboBox { Location = new Point(520, 40), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSpecialty.DataSource = Enum.GetValues(typeof(DoctorSpecialty));
            Button btnAdd = new Button { Text = "Добавить", Location = new Point(520, 70), Width = 200 };

            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtDoctorName.Text)) return;
                var spec = cmbSpecialty.SelectedItem != null ? (DoctorSpecialty)cmbSpecialty.SelectedItem : DoctorSpecialty.None;
                _doctorRepo.Create(new Doctor { FullName = txtDoctorName.Text, Specialty = spec });
                txtDoctorName.Clear();
                LoadAllData();
            };

            page.Controls.AddRange(new Control[] { gridDoctors, txtDoctorName, cmbSpecialty, btnAdd });
            tabControl.TabPages.Add(page);
        }

        private void CreateServicesTab()
        {
            TabPage page = new TabPage("Услуги");
            gridServices = new DataGridView { Location = new Point(10, 10), Size = new Size(500, 400), AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            txtServiceName = new TextBox { Location = new Point(520, 10), Width = 200, PlaceholderText = "Название" };
            clbServiceType = new CheckedListBox { Location = new Point(520, 40), Width = 200, Height = 100, CheckOnClick = true };
            clbServiceType.Items.AddRange(Enum.GetValues(typeof(ServiceType)).Cast<object>().ToArray());
            Button btnAdd = new Button { Text = "Добавить", Location = new Point(520, 150), Width = 200 };

            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtServiceName.Text)) return;
                ServiceType type = ServiceType.None;
                foreach (var item in clbServiceType.CheckedItems) type |= (ServiceType)item;
                _serviceRepo.Create(new MedicalService { Name = txtServiceName.Text, Type = type });
                txtServiceName.Clear();
                LoadAllData();
            };

            page.Controls.AddRange(new Control[] { gridServices, txtServiceName, clbServiceType, btnAdd });
            tabControl.TabPages.Add(page);
        }

        private void CreatePlansTab()
        {
            TabPage page = new TabPage("Планы лечения");
            gridPlans = new DataGridView { Location = new Point(10, 10), Size = new Size(500, 400), AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            
            cmbPlanPatient = new ComboBox { Location = new Point(520, 10), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPlanDoctor = new ComboBox { Location = new Point(520, 40), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPlanService = new ComboBox { Location = new Point(520, 70), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            numPlanQuantity = new NumericUpDown { Location = new Point(520, 100), Width = 200, Minimum = 1 };
            Button btnAddService = new Button { Text = "В план", Location = new Point(520, 130), Width = 200 };
            Button btnSavePlan = new Button { Text = "Сохранить План", Location = new Point(520, 170), Width = 200 };

            btnAddService.Click += (s, e) => {
                if (cmbPlanService.SelectedValue is not int serviceId) return;
                currentPlanServices.Add(new PlanService { ServiceId = serviceId, Quantity = (int)numPlanQuantity.Value });
                MessageBox.Show("Услуга добавлена в план. Теперь нажмите Сохранить.");
            };

            btnSavePlan.Click += (s, e) => {
                if (cmbPlanPatient.SelectedValue is not int patientId || cmbPlanDoctor.SelectedValue is not int doctorId || currentPlanServices.Count == 0)
                {
                    MessageBox.Show("Выберите пациента, врача и хотя бы одну услугу!");
                    return;
                }

                try
                {
                    var plan = new TreatmentPlan {
                        PatientId = patientId,
                        DoctorId = doctorId,
                        DateCreated = DateTime.Now,
                        Services = currentPlanServices.ToList()
                    };
                    
                    _planRepo.Create(plan);
                    currentPlanServices.Clear();
                    MessageBox.Show("План лечения сохранен!");
                    LoadAllData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            };

            page.Controls.AddRange(new Control[] { gridPlans, cmbPlanPatient, cmbPlanDoctor, cmbPlanService, numPlanQuantity, btnAddService, btnSavePlan });
            tabControl.TabPages.Add(page);
        }
    }
}