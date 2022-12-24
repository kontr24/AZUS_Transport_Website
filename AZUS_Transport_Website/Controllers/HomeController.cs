using AZUS_Transport_Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Word = Microsoft.Office.Interop.Word;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using OfficeOpenXml.Style;

namespace AZUS_Transport_Website.Controllers
{
    public class HomeController : Controller
    {
        private static int _MaxId = 0;
        //загрузка фотографий с сервера
        public FilePathResult GetImage(int Id)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            Applications applications = db.Applications.Include(g => g.Cars).FirstOrDefault(g => g.Id == Id);

            if (applications != null)
            {

                return File(@"~/Content/Images/Cars/" + applications.Cars.ImageMimeType, "image/jpeg");
                //return File(product.ImageData, product.ImageMimeType);
            }
            else
            {
                return null;
            }
        }
        //загрузка фотографий с сервера


        //word
        public readonly string TemplateFileName = @"~/Content/Word/Pattern.docx";// Расположение шаблона   
        [HttpPost]
        public FileResult WordSave(int? idApp, string statusDone = null)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            Applications application = new Applications();
            application = db.Applications.Find(idApp);

            var usr = db.Users.FirstOrDefault(x => x.Id == application.UserID);
            var crs = db.Cars.FirstOrDefault(x => x.Id == application.CarID);
            var mdCrs = db.ModelCars.FirstOrDefault(x => x.Id == crs.ModelCarID);

            var num = application.Id.ToString();
            var status = statusDone;
            var clientFullName = usr.SurName + " " + usr.Name + " " + usr.Partonymic;
            var email = usr.Email;
            var post = usr.Post;
            var dvc = db.Divisions.FirstOrDefault(x => x.Id == usr.DivisionID);
            var division = dvc.Name;
            var building = dvc.Building;
            var room = usr.Room.ToString();
            var workPhone = usr.WorkPhone;
            var mobilePhone = usr.MobilePhone;
            var usrDrc = db.Users.FirstOrDefault(x => x.DivisionID == usr.DivisionID && x.StatusID == 3);
            var directorFullName = usrDrc.SurName + " " + usrDrc.Name + " " + usrDrc.Partonymic;
            var usrEcn = db.Users.FirstOrDefault(x => x.DivisionID == usr.DivisionID && x.StatusID == 4);
            var economistFullName = usrEcn.SurName + " " + usrEcn.Name + " " + usrEcn.Partonymic;

            var daysWorkerOrWeekend = string.Format("{0}", application.Days.Value ? "Рабочий" : "Выходной");

            var startDate = String.Format("{0:d} - {0:t}", application.StartDate);
            var endDate = String.Format("{0:d} - {0:t}", application.EndDate);
            var intercityСity = string.Format("{0}", application.IntercityСity.Value ? "Город" : "Межгород");
            var tpCr = db.TypeCars.FirstOrDefault(x => x.Id == application.TypeCarID);
            var typeCar = tpCr.Name;
            var passenger = application.QuantityPassengers.ToString();
            var cargo = application.CargoWeight.ToString();
            var placeSubmission = application.PlaceSubmission;
            var route = application.Route;
            var purposeUsingTransport = application.PurposeUsingTransport;


            var wordApp = new Word.Application();//создание приложения
            wordApp.Visible = false;

            try
            {
                var fullPath = Server.MapPath(TemplateFileName);//путь к файлу
                var wordDocument = wordApp.Documents.Open(fullPath); //открываем файл word
                // замена меток в Word на переменные
                ReplaceWordStub("{Num}", num, wordDocument);
                ReplaceWordStub("{Status}", status, wordDocument);
                ReplaceWordStub("{ClientFullName}", clientFullName, wordDocument);
                ReplaceWordStub("{Emeil}", email, wordDocument);
                ReplaceWordStub("{Post}", post, wordDocument);
                ReplaceWordStub("{Division}", division, wordDocument);
                ReplaceWordStub("{Building}", building, wordDocument);
                ReplaceWordStub("{Room}", room, wordDocument);
                ReplaceWordStub("{WorkPhone}", workPhone, wordDocument);
                ReplaceWordStub("{MobilePhone}", mobilePhone, wordDocument);
                ReplaceWordStub("{DirectorFullName}", directorFullName, wordDocument);
                ReplaceWordStub("{EconomistFullName}", economistFullName, wordDocument);
                ReplaceWordStub("{DaysWorkerOrWeekend}", daysWorkerOrWeekend, wordDocument);
                ReplaceWordStub("{StartDate}", startDate, wordDocument);
                ReplaceWordStub("{EndDate}", endDate, wordDocument);
                ReplaceWordStub("{IntercityСity}", intercityСity, wordDocument);
                ReplaceWordStub("{TypeCar}", typeCar, wordDocument);
                ReplaceWordStub("{Passenger}", passenger, wordDocument);
                ReplaceWordStub("{Cargo}", cargo, wordDocument);
                ReplaceWordStub("{PlaceSubmission}", placeSubmission, wordDocument);
                ReplaceWordStub("{Route}", route, wordDocument);
                ReplaceWordStub("{PurposeUsingTransport}", purposeUsingTransport, wordDocument);
                if (application.CarID != 1)
                {
                    var designatedTransport = "Назначенный транспорт";
                    var transport = mdCrs.Name + " - " + crs.RegisterSign;
                    ReplaceWordStub("{DesignatedTransport}", designatedTransport, wordDocument);
                    ReplaceWordStub("{Transport}", transport, wordDocument);
                }
                else
                {
                    var designatedTransport = "";
                    var transport = "";
                    ReplaceWordStub("{DesignatedTransport}", designatedTransport, wordDocument);
                    ReplaceWordStub("{Transport}", transport, wordDocument);
                }
                // замена меток в Word на переменные
                //удаление файла, если он есть в папке
                FileInfo fileInf = new FileInfo(Server.MapPath(@"~/Content/Word/" + application.UserID + ".docx"));
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                    // альтернатива с помощью класса File
                    // File.Delete(path);
                }
                //удаление файла, если он есть в папке
                //сохранение файла
                wordDocument.SaveAs(Server.MapPath(@"~/Content/Word/" + application.UserID + ".docx"));
                //сохранение файла

                //остановить процесс
                wordDocument.Close();
                //остановить процесс

                // Путь к файлу
                string file_path = Server.MapPath(@"~/Content/Word/" + application.UserID + ".docx");
                // Тип файла - content-type
                string file_type = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                // Имя файла - необязательно
                string file_name = "Заявка № " + application.Id.ToString()/*.Substring(0, application.Id.ToString().IndexOf(@"    ["))*/ + ".docx";

                //загрузка сформированного файла
                return File(file_path, file_type, file_name);
                //загрузка сформированного файла

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return null;

        }



        public void ReplaceWordStub(string stubToReplace, string text, Word.Document wordDocument) // замена меток на нашу информациию
        {
            // получить содержимое документа word
            var range = wordDocument.Content;
            range.Find.ClearFormatting();// очистить поиски в документе
            range.Find.Execute(FindText: stubToReplace, ReplaceWith: text);//передача параметров (FindText-то, что хотим найти внутри документа;
                                                                           //ReplaceWith-то, чем хотим заменить)

        }
        //word

        [HttpPost]
        public FileResult ExcelSave(bool archiveTrue = true, bool archiveFalse = false, string j = null)
        {
            //Создаём приложение.
            Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();


            ////чтение с excel
            //Microsoft.Office.Interop.Excel.Application ExcelApp1 = new Microsoft.Office.Interop.Excel.Application();
            ////Открываем книгу.
            //Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ExcelApp1.Workbooks.Open(Server.MapPath(@"~/Content/Excel/18.xlsx"), 0, false, 5, "", "", false, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            ////Выбираем таблицу(лист).
            //Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
            //ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)ObjWorkBook.Sheets[1];

            //Microsoft.Office.Interop.Excel.Range range = ObjWorkSheet.Cells[22, 1]/* get_Range("B22")*/;
            //string adg = range.Text;
            //string ty = ObjWorkSheet.Cells[22, 1].Text;
            //чтение с excel

            try
            {
                //Книга.
                var ExcelWorkBook = ExcelApp.Workbooks.Add(System.Reflection.Missing.Value);
                if (Users.mode == (int)Users.Status.Director ||
                    Users.mode == (int)Users.Status.Economist)
                {

                    using (var db = new ASUZ_Transport_DBEntitie())
                    {
                        var usrDrc = db.Users.FirstOrDefault(x => x.Id == Users.UserID);
                        var usrEcn = db.Users.FirstOrDefault(x => x.DivisionID == usrDrc.DivisionID && x.StatusID == 4);

                        var usrDr = db.Users.FirstOrDefault(x => x.DivisionID == usrEcn.DivisionID && x.StatusID == 3);
                        //if (usrDr == null)
                        //{
                        //    MessageBox.Show("Руководитель вашего подразделения не зарегистрирован в системе! Вы сможете сохранить данные только после его регистрации!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    return;
                        //}
                        //if (usrEcn == null)
                        //{
                        //    MessageBox.Show("Экономист вашего подразделения не зарегистрирован в системе! Вы сможете сохранить данные только после его регистрации!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    return;
                        //}
                        //посчитать количество определенных одинаковых данных
                        var firstColumnValuesDirector = db.Applications.Include(x => x.Users).Where(x => x.Users.DivisionID == usrDrc.DivisionID).AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.DirectorStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultDirector = firstColumnValuesDirector.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int agreedDirector;
                        resultDirector.TryGetValue(1, out agreedDirector);
                        int considerationDirector;
                        resultDirector.TryGetValue(3, out considerationDirector);
                        int rejectedDirector;
                        resultDirector.TryGetValue(2, out rejectedDirector);


                        var firstColumnValuesEconomist = db.Applications.Include(x => x.Users).Where(x => x.Users.DivisionID == usrEcn.DivisionID).AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.EconomistStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultEconomist = firstColumnValuesEconomist.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int agreedEconomist;
                        resultEconomist.TryGetValue(1, out agreedEconomist);
                        int considerationEconomist;
                        resultEconomist.TryGetValue(3, out considerationEconomist);
                        int rejectedEconomist;
                        resultEconomist.TryGetValue(2, out rejectedEconomist);

                        var firstColumnValuesDepartment = db.Applications.Include(x => x.Users).Where(x => x.Users.Id == usrDrc.Id).AsEnumerable().Where(x => (x.DateCreation.AddDays(1) > DateTime.Now && (x.IntercityСity != true || x.Days != true))).GroupBy(x => x.DepartmentStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultDepartment = firstColumnValuesDepartment.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int agreedDepartment;
                        resultDepartment.TryGetValue(1, out agreedDepartment);
                        int considerationDepartment;
                        resultDepartment.TryGetValue(3, out considerationDepartment);
                        int rejectedDepartment;
                        resultDepartment.TryGetValue(2, out rejectedDepartment);

                        var firstColumnValuesDispatcherNIIARStatusDone = db.Applications.Include(x => x.Users).Where(x => x.Users.DivisionID == usrDrc.DivisionID).AsEnumerable().Where(x => (x.DateCreation.AddDays(1) > DateTime.Now)).GroupBy(x => x.DispatcherNIIAR_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultDispatcherNIIARStatusDone = firstColumnValuesDispatcherNIIARStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int agreedDispatcherNIIARStatusDone;
                        resultDispatcherNIIARStatusDone.TryGetValue(1, out agreedDispatcherNIIARStatusDone);
                        int considerationDispatcherNIIARStatusDone;
                        resultDispatcherNIIARStatusDone.TryGetValue(3, out considerationDispatcherNIIARStatusDone);
                        int rejectedDispatcherNIIARStatusDone;
                        resultDispatcherNIIARStatusDone.TryGetValue(2, out rejectedDispatcherNIIARStatusDone);

                        var firstColumnValuesDispatcherATAStatusDone = db.Applications.Include(x => x.Users).Where(x => x.Users.DivisionID == usrDrc.DivisionID).AsEnumerable().Where(x => (x.DateCreation.AddDays(1) > DateTime.Now)).GroupBy(x => x.DispatcherATA_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultDispatcherATAStatusDone = firstColumnValuesDispatcherATAStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int ExecutedDispatcherATAStatusDone;
                        resultDispatcherATAStatusDone.TryGetValue(4, out ExecutedDispatcherATAStatusDone);
                        int NotExecutedDispatcherATAStatusDone;
                        resultDispatcherATAStatusDone.TryGetValue(5, out NotExecutedDispatcherATAStatusDone);
                        //посчитать количество определенных одинаковых данных
                        var firstColumnValuesSumm = db.Applications.Include(x => x.Users).Where(x => x.Users.DivisionID == usrDrc.DivisionID).AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).Count();


                        //массив с 1 до 35 
                        int[] number = Enumerable.Range(1, 35).ToArray();
                        //массив с 1 до 35 

                        //Раскраска строки с 1 по 35 столбец
                        for (int i = 1; i < number.Count() + 1; i++)
                        {
                            ExcelApp.Cells[21, i].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[21, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                            ExcelApp.Cells[21, i].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        }
                        //Раскраска строки с 1 по 35 столбец

                        //цвет ячейки
                        ExcelApp.Cells[1, 1].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);
                        ExcelApp.Cells[1, 2].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);

                        ExcelApp.Cells[2, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[2, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[17, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[18, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[17, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[18, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[20, 1].Interior.Color = ColorTranslator.ToOle(Color.DarkGray);
                        //цвет ячейки


                        //строки вправо
                        for (int i = 1; i <= 18; i++)
                        {
                            ExcelApp.Cells[i, 2].HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                        //строки вправо

                        //Отчёт
                        ExcelApp.Cells[1, 2] = String.Format("{0:d} - {0:t}", DateTime.Now.AddDays(-1)) + " — " + String.Format("{0:d} - {0:t}", DateTime.Now);
                        ExcelApp.Cells[2, 1].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[2, 2].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[2, 1] = "Текущий статус";
                        ExcelApp.Cells[2, 2] = "Количество";
                        ExcelApp.Cells[3, 2] = considerationDirector;
                        ExcelApp.Cells[4, 2] = considerationEconomist;
                        ExcelApp.Cells[5, 2] = considerationDepartment;
                        ExcelApp.Cells[6, 2] = considerationDispatcherNIIARStatusDone;
                        ExcelApp.Cells[7, 2] = rejectedDirector;
                        ExcelApp.Cells[8, 2] = rejectedEconomist;
                        ExcelApp.Cells[9, 2] = rejectedDepartment;
                        ExcelApp.Cells[10, 2] = rejectedDispatcherNIIARStatusDone;
                        ExcelApp.Cells[11, 2] = agreedDirector;
                        ExcelApp.Cells[12, 2] = agreedEconomist;
                        ExcelApp.Cells[13, 2] = agreedDepartment;
                        ExcelApp.Cells[14, 2] = agreedDispatcherNIIARStatusDone;
                        ExcelApp.Cells[15, 2] = ExecutedDispatcherATAStatusDone;
                        ExcelApp.Cells[16, 2] = NotExecutedDispatcherATAStatusDone;
                        ExcelApp.Cells[17, 2].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[18, 2].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[17, 2] = firstColumnValuesSumm;
                        ExcelApp.Cells[18, 2] = String.Format("{0:d} - {0:t}", DateTime.Now);

                        ExcelApp.Cells[1, 1].Font.Bold = true;
                        ExcelApp.Cells[1, 2].Font.Bold = true;
                        //Отчёт
                        //строки влево
                        for (int i = 1; i <= 18; i++)
                        {
                            ExcelApp.Cells[i, 1].HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                        //строки влево
                        //Отчёт
                        ExcelApp.Cells[1, 1].Font.Size = 16; //размер шрифта 
                        ExcelApp.Cells[1, 2].Font.Size = 16;
                        ExcelApp.Cells[20, 1] = "Заявки";
                        ExcelApp.Cells[20, 1].Font.Size = 16;
                        ExcelApp.Cells[20, 1].Font.Bold = true;
                        ExcelApp.Cells[1, 1] = "Отчёт по заявкам за период: ";
                        ExcelApp.Cells[3, 1] = "На рассмотрении у руководителей";
                        ExcelApp.Cells[4, 1] = "На рассмотрении у экономистов";
                        ExcelApp.Cells[5, 1] = "На рассмотрении у ДИД";
                        ExcelApp.Cells[6, 1] = "На рассмотрении у диспетчера НИИАР";
                        ExcelApp.Cells[7, 1] = "Отклонены руководителями";
                        ExcelApp.Cells[8, 1] = "Отклонены экономистами";
                        ExcelApp.Cells[9, 1] = "Отклонены ДИД";
                        ExcelApp.Cells[10, 1] = "Отклонены  диспетчером НИИАР";
                        ExcelApp.Cells[11, 1] = "Согласовано руководителями";
                        ExcelApp.Cells[12, 1] = "Согласовано экономистами";
                        ExcelApp.Cells[13, 1] = "Согласовано ДИД";
                        ExcelApp.Cells[14, 1] = "Согласовано диспетчером НИИАР";
                        ExcelApp.Cells[15, 1] = "Исполнено";
                        ExcelApp.Cells[16, 1] = "Не исполнено";
                        ExcelApp.Cells[17, 1].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[17, 1] = "Количество поступивших заявок";
                        ExcelApp.Cells[18, 1].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[18, 1] = "Дата оформления отчёта";
                        //Отчёт

                        ////Для названия столбцов из datagridview
                        //for (int i = 1; i < dgvApplication.Columns.Count + 1; i++)
                        //{
                        //    ExcelApp.Cells[21, i] = dgvApplication.Columns[i - 1].HeaderText;
                        //}
                        ////Для названия столбцов  datagridview





                        //for (int i = 0; i < dgvApplication.Rows.Count; i++)
                        //{
                        //    for (int j = 0; j < dgvApplication.ColumnCount; j++)
                        //    {
                        //        ExcelApp.Cells[i + 22, j + 1] = dgvApplication.Rows[i].Cells[j].Value;

                        //    }
                        //}

                        //данные из модели
                        List<Applications> applications = db.Applications.ToList();
                        MethodsRepository methodsRepository = new MethodsRepository();
                        applications = methodsRepository.GetApplications(archiveTrue, archiveFalse);

                        List<Applications> appL = new List<Applications>();
                        //данные из модели

                        //добавление данных из модели
                        for (int i = 0; i < applications.Count(); i++)
                        {
                            foreach (var app in applications)
                            {
                                //    int[] appM = { app.Id };
                                //    appL.Add(app.Id);
                                appL.Add(app);

                            }
                            for (int ap = 0; ap < appL.Count; ap++)
                            {
                                ExcelApp.Cells[i + 22, 1] = appL[i].Id;
                                ExcelApp.Cells[i + 22, 2] = appL[i].Users.SurName + " " + appL[i].Users.Name + " " + appL[i].Users.Partonymic;
                                ExcelApp.Cells[i + 22, 3] = appL[i].Users.Email;
                                ExcelApp.Cells[i + 22, 4] = appL[i].Users.Post;
                                ExcelApp.Cells[i + 22, 5] = appL[i].Users.Divisions.Name;
                                ExcelApp.Cells[i + 22, 6] = usrDrc.SurName + " " + usrDrc.Name + " " + usrDrc.Partonymic;
                                ExcelApp.Cells[i + 22, 7] = usrEcn.SurName + " " + usrEcn.Name + " " + usrEcn.Partonymic;
                                ExcelApp.Cells[i + 22, 8] = appL[i].CPC;
                                ExcelApp.Cells[i + 22, 9] = string.Format("{0}", appL[i].IntercityСity.Value ? "Город" : "Межгород");
                                ExcelApp.Cells[i + 22, 10] = appL[i].PurposeUsingTransport;
                                ExcelApp.Cells[i + 22, 11] = string.Format("{0}", appL[i].Days.Value ? "Рабочий" : "Выходной");
                                ExcelApp.Cells[i + 22, 12] = appL[i].Users.WorkPhone;
                                ExcelApp.Cells[i + 22, 13] = appL[i].Users.MobilePhone;
                                ExcelApp.Cells[i + 22, 14] = appL[i].DateCreation;
                                ExcelApp.Cells[i + 22, 15] = appL[i].StartDate;
                                ExcelApp.Cells[i + 22, 16] = appL[i].EndDate;
                                ExcelApp.Cells[i + 22, 17] = appL[i].TypeCars.Name;
                                ExcelApp.Cells[i + 22, 18] = appL[i].Cars.ModelCars.Name;
                                ExcelApp.Cells[i + 22, 19] = appL[i].Cars.RegisterSign;
                                ExcelApp.Cells[i + 22, 20] = appL[i].QuantityPassengers;
                                ExcelApp.Cells[i + 22, 21] = appL[i].CargoWeight;
                                ExcelApp.Cells[i + 22, 22] = appL[i].PlaceSubmission;
                                ExcelApp.Cells[i + 22, 23] = appL[i].Route;
                                ExcelApp.Cells[i + 22, 24] = appL[i].CommentClient;
                                ExcelApp.Cells[i + 22, 25] = appL[i].СommentDirector;
                                ExcelApp.Cells[i + 22, 26] = appL[i].СommentEconomist;
                                ExcelApp.Cells[i + 22, 27] = appL[i].СommentDepartment;
                                ExcelApp.Cells[i + 22, 28] = appL[i].СommentDispatcherNIIAR;
                                ExcelApp.Cells[i + 22, 29] = appL[i].СommentDispatcherATA;
                                ExcelApp.Cells[i + 22, 30] = appL[i].StatusesDone.Name;
                                ExcelApp.Cells[i + 22, 31] = appL[i].StatusesDone1.Name;
                                ExcelApp.Cells[i + 22, 32] = appL[i].StatusesDone3.Name;
                                ExcelApp.Cells[i + 22, 33] = appL[i].StatusesDone2.Name;
                                ExcelApp.Cells[i + 22, 34] = appL[i].StatusesDone4.Name;
                                ExcelApp.Cells[i + 22, 35] = appL[i].ApplicationJoin;
                            }

                        }
                        //добавление данных из модели
                        //for (int i = 0; i < dgvApplication.Rows.Count; i++)
                        //{
                        //    for (int j = 0; j < dgvApplication.ColumnCount; j++)
                        //    {
                        //        ExcelApp.Cells[i + 22, j + 1] = dgvApplication.Rows[i].Cells[j].Value;

                        //    }
                        //}


                    }
                }

                if (Users.mode == (int)Users.Status.Department ||
              Users.mode == (int)Users.Status.DispatcherNIIAR)
                {
                    using (var db = new ASUZ_Transport_DBEntitie())
                    {
                        //посчитать количество определенных одинаковых данных
                        var firstColumnValuesDirector = db.Applications.AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.DirectorStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultDirector = firstColumnValuesDirector.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int agreedDirector;
                        resultDirector.TryGetValue(1, out agreedDirector);
                        int considerationDirector;
                        resultDirector.TryGetValue(3, out considerationDirector);
                        int rejectedDirector;
                        resultDirector.TryGetValue(2, out rejectedDirector);


                        var firstColumnValuesEconomist = db.Applications.AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.EconomistStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultEconomist = firstColumnValuesEconomist.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int agreedEconomist;
                        resultEconomist.TryGetValue(1, out agreedEconomist);
                        int considerationEconomist;
                        resultEconomist.TryGetValue(3, out considerationEconomist);
                        int rejectedEconomist;
                        resultEconomist.TryGetValue(2, out rejectedEconomist);

                        var firstColumnValuesDepartment = db.Applications.AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now && (x.IntercityСity != true || x.Days != true)).GroupBy(x => x.DepartmentStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultDepartment = firstColumnValuesDepartment.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int agreedDepartment;
                        resultDepartment.TryGetValue(1, out agreedDepartment);
                        int considerationDepartment;
                        resultDepartment.TryGetValue(3, out considerationDepartment);
                        int rejectedDepartment;
                        resultDepartment.TryGetValue(2, out rejectedDepartment);

                        var firstColumnValuesDispatcherNIIARStatusDone = db.Applications.AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.DispatcherNIIAR_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultDispatcherNIIARStatusDone = firstColumnValuesDispatcherNIIARStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int agreedDispatcherNIIARStatusDone;
                        resultDispatcherNIIARStatusDone.TryGetValue(1, out agreedDispatcherNIIARStatusDone);
                        int considerationDispatcherNIIARStatusDone;
                        resultDispatcherNIIARStatusDone.TryGetValue(3, out considerationDispatcherNIIARStatusDone);
                        int rejectedDispatcherNIIARStatusDone;
                        resultDispatcherNIIARStatusDone.TryGetValue(2, out rejectedDispatcherNIIARStatusDone);

                        var firstColumnValuesDispatcherATAStatusDone = db.Applications.AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.DispatcherATA_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                        Dictionary<int, int> resultDispatcherATAStatusDone = firstColumnValuesDispatcherATAStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                        int ExecutedDispatcherATAStatusDone;
                        resultDispatcherATAStatusDone.TryGetValue(4, out ExecutedDispatcherATAStatusDone);
                        int NotExecutedDispatcherATAStatusDone;
                        resultDispatcherATAStatusDone.TryGetValue(5, out NotExecutedDispatcherATAStatusDone);
                        //посчитать количество определенных одинаковых данных
                        var firstColumnValuesSumm = db.Applications.AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).Count();


                        int[] number = Enumerable.Range(1, 35).ToArray();

                        for (int i = 1; i < number.Count() + 1; i++)
                        {
                            ExcelApp.Cells[21, i].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[21, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                            ExcelApp.Cells[21, i].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        }
                        //цвет ячейки
                        ExcelApp.Cells[1, 1].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);
                        ExcelApp.Cells[1, 2].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);

                        ExcelApp.Cells[2, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[2, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[17, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[18, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[17, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[18, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                        ExcelApp.Cells[20, 1].Interior.Color = ColorTranslator.ToOle(Color.DarkGray);
                        //цвет ячейки


                        //строки вправо
                        for (int i = 1; i <= 18; i++)
                        {
                            ExcelApp.Cells[i, 2].HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                        //строки вправо

                        ExcelApp.Cells[1, 2] = String.Format("{0:d} - {0:t}", DateTime.Now.AddDays(-1)) + " — " + String.Format("{0:d} - {0:t}", DateTime.Now);

                        ExcelApp.Cells[2, 1].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[2, 2].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[2, 1] = "Текущий статус";
                        ExcelApp.Cells[2, 2] = "Количество";
                        ExcelApp.Cells[3, 2] = considerationDirector;
                        ExcelApp.Cells[4, 2] = considerationEconomist;
                        ExcelApp.Cells[5, 2] = considerationDepartment;
                        ExcelApp.Cells[6, 2] = considerationDispatcherNIIARStatusDone;
                        ExcelApp.Cells[7, 2] = rejectedDirector;
                        ExcelApp.Cells[8, 2] = rejectedEconomist;
                        ExcelApp.Cells[9, 2] = rejectedDepartment;
                        ExcelApp.Cells[10, 2] = rejectedDispatcherNIIARStatusDone;
                        ExcelApp.Cells[11, 2] = agreedDirector;
                        ExcelApp.Cells[12, 2] = agreedEconomist;
                        ExcelApp.Cells[13, 2] = agreedDepartment;
                        ExcelApp.Cells[14, 2] = agreedDispatcherNIIARStatusDone;
                        ExcelApp.Cells[15, 2] = ExecutedDispatcherATAStatusDone;
                        ExcelApp.Cells[16, 2] = NotExecutedDispatcherATAStatusDone;
                        ExcelApp.Cells[17, 2].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[18, 2].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[17, 2] = firstColumnValuesSumm;
                        ExcelApp.Cells[18, 2] = String.Format("{0:d} - {0:t}", DateTime.Now);

                        ExcelApp.Cells[1, 1].Font.Bold = true;
                        ExcelApp.Cells[1, 2].Font.Bold = true;

                        //строки влево
                        for (int i = 1; i <= 18; i++)
                        {
                            ExcelApp.Cells[i, 1].HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                        //строки влево
                        ExcelApp.Cells[1, 1].Font.Size = 16; //размер шрифта 
                        ExcelApp.Cells[1, 2].Font.Size = 16;
                        ExcelApp.Cells[20, 1] = "Заявки";
                        ExcelApp.Cells[20, 1].Font.Size = 16;
                        ExcelApp.Cells[20, 1].Font.Bold = true;
                        ExcelApp.Cells[1, 1] = "Отчёт по заявкам за период: ";
                        ExcelApp.Cells[3, 1] = "На рассмотрении у руководителей";
                        ExcelApp.Cells[4, 1] = "На рассмотрении у экономистов";
                        ExcelApp.Cells[5, 1] = "На рассмотрении у ДИД";
                        ExcelApp.Cells[6, 1] = "На рассмотрении у диспетчера НИИАР";
                        ExcelApp.Cells[7, 1] = "Отклонены руководителями";
                        ExcelApp.Cells[8, 1] = "Отклонены экономистами";
                        ExcelApp.Cells[9, 1] = "Отклонены ДИД";
                        ExcelApp.Cells[10, 1] = "Отклонены  диспетчером НИИАР";
                        ExcelApp.Cells[11, 1] = "Согласовано руководителями";
                        ExcelApp.Cells[12, 1] = "Согласовано экономистами";
                        ExcelApp.Cells[13, 1] = "Согласовано ДИД";
                        ExcelApp.Cells[14, 1] = "Согласовано диспетчером НИИАР";
                        ExcelApp.Cells[15, 1] = "Исполнено";
                        ExcelApp.Cells[16, 1] = "Не исполнено";
                        ExcelApp.Cells[17, 1].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[17, 1] = "Количество поступивших заявок";
                        ExcelApp.Cells[18, 1].Font.Bold = true;//жирный шрифт
                        ExcelApp.Cells[18, 1] = "Дата оформления отчёта";



                        List<Applications> applications = db.Applications.ToList();
                        MethodsRepository methodsRepository = new MethodsRepository();
                        applications = methodsRepository.GetApplications(archiveTrue, archiveFalse);

                        List<Applications> appL = new List<Applications>();

                        for (int i = 0; i < applications.Count(); i++)
                        {
                            foreach (var app in applications)
                            {
                                //    int[] appM = { app.Id };
                                //    appL.Add(app.Id);
                                appL.Add(app);

                            }

                            for (int ap = 0; ap < appL.Count; ap++)
                            {
                                ExcelApp.Cells[i + 22, 1] = appL[i].Id;
                                ExcelApp.Cells[i + 22, 2] = appL[i].Users.SurName + " " + appL[i].Users.Name + " " + appL[i].Users.Partonymic;
                                ExcelApp.Cells[i + 22, 3] = appL[i].Users.Email;
                                ExcelApp.Cells[i + 22, 4] = appL[i].Users.Post;
                                ExcelApp.Cells[i + 22, 5] = appL[i].Users.Divisions.Name;


                                foreach (var usr in appL.Where(x => x.Users.Id == appL[i].UserID))
                                {
                                    foreach (var usrDrc in db.Users.Where(x => x.DivisionID == usr.Users.DivisionID && x.StatusID == 3))
                                    {
                                        ExcelApp.Cells[i + 22, 6] = usrDrc.SurName + " " + usrDrc.Name + " " + usrDrc.Partonymic;
                                    }
                                }
                                foreach (var usr in appL.Where(x => x.Users.Id == appL[i].UserID))
                                {
                                    foreach (var usrEcn in db.Users.Where(x => x.DivisionID == usr.Users.DivisionID && x.StatusID == 4))
                                    {

                                        ExcelApp.Cells[i + 22, 7] = usrEcn.SurName + " " + usrEcn.Name + " " + usrEcn.Partonymic;
                                    }
                                }
                                ExcelApp.Cells[i + 22, 8] = appL[i].CPC;
                                ExcelApp.Cells[i + 22, 9] = string.Format("{0}", appL[i].IntercityСity.Value ? "Город" : "Межгород");
                                ExcelApp.Cells[i + 22, 10] = appL[i].PurposeUsingTransport;
                                ExcelApp.Cells[i + 22, 11] = string.Format("{0}", appL[i].Days.Value ? "Рабочий" : "Выходной");
                                ExcelApp.Cells[i + 22, 12] = appL[i].Users.WorkPhone;
                                ExcelApp.Cells[i + 22, 13] = appL[i].Users.MobilePhone;
                                ExcelApp.Cells[i + 22, 14] = appL[i].DateCreation;
                                ExcelApp.Cells[i + 22, 15] = appL[i].StartDate;
                                ExcelApp.Cells[i + 22, 16] = appL[i].EndDate;
                                ExcelApp.Cells[i + 22, 17] = appL[i].TypeCars.Name;
                                ExcelApp.Cells[i + 22, 18] = appL[i].Cars.ModelCars.Name;
                                ExcelApp.Cells[i + 22, 19] = appL[i].Cars.RegisterSign;
                                ExcelApp.Cells[i + 22, 20] = appL[i].QuantityPassengers;
                                ExcelApp.Cells[i + 22, 21] = appL[i].CargoWeight;
                                ExcelApp.Cells[i + 22, 22] = appL[i].PlaceSubmission;
                                ExcelApp.Cells[i + 22, 23] = appL[i].Route;
                                ExcelApp.Cells[i + 22, 24] = appL[i].CommentClient;
                                ExcelApp.Cells[i + 22, 25] = appL[i].СommentDirector;
                                ExcelApp.Cells[i + 22, 26] = appL[i].СommentEconomist;
                                ExcelApp.Cells[i + 22, 27] = appL[i].СommentDepartment;
                                ExcelApp.Cells[i + 22, 28] = appL[i].СommentDispatcherNIIAR;
                                ExcelApp.Cells[i + 22, 29] = appL[i].СommentDispatcherATA;
                                ExcelApp.Cells[i + 22, 30] = appL[i].StatusesDone.Name;
                                ExcelApp.Cells[i + 22, 31] = appL[i].StatusesDone1.Name;
                                ExcelApp.Cells[i + 22, 32] = appL[i].StatusesDone3.Name;
                                ExcelApp.Cells[i + 22, 33] = appL[i].StatusesDone2.Name;
                                ExcelApp.Cells[i + 22, 34] = appL[i].StatusesDone4.Name;
                                ExcelApp.Cells[i + 22, 35] = appL[i].ApplicationJoin;
                            }

                        }
                    }

                }

                if (Users.mode == (int)Users.Status.DispatcherATA)
                {
                    if (archiveTrue == true)
                    {

                        using (var db = new ASUZ_Transport_DBEntitie())
                        {
                            //посчитать количество определенных одинаковых данных
                            var firstColumnValuesDirector = db.Applications.AsEnumerable().Where(x => (
                       (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID != 3) && x.DispatcherATA_StatusDoneID == 3) || (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID == 3) && x.DispatcherATA_StatusDoneID == 3).GroupBy(x => x.DirectorStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDirector = firstColumnValuesDirector.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDirector;
                            resultDirector.TryGetValue(1, out agreedDirector);
                            int considerationDirector;
                            resultDirector.TryGetValue(3, out considerationDirector);
                            int rejectedDirector;
                            resultDirector.TryGetValue(2, out rejectedDirector);


                            var firstColumnValuesEconomist = db.Applications.AsEnumerable().Where(x => (
                       (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID != 3) && x.DispatcherATA_StatusDoneID == 3) || (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID == 3) && x.DispatcherATA_StatusDoneID == 3).GroupBy(x => x.EconomistStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultEconomist = firstColumnValuesEconomist.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedEconomist;
                            resultEconomist.TryGetValue(1, out agreedEconomist);
                            int considerationEconomist;
                            resultEconomist.TryGetValue(3, out considerationEconomist);
                            int rejectedEconomist;
                            resultEconomist.TryGetValue(2, out rejectedEconomist);

                            var firstColumnValuesDepartment = db.Applications.AsEnumerable().Where(x => ((
                       (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID != 3) && x.DispatcherATA_StatusDoneID == 3) || (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID == 3) && x.DispatcherATA_StatusDoneID == 3) && (x.IntercityСity != true || x.Days != true)).GroupBy(x => x.DepartmentStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDepartment = firstColumnValuesDepartment.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDepartment;
                            resultDepartment.TryGetValue(1, out agreedDepartment);
                            int considerationDepartment;
                            resultDepartment.TryGetValue(3, out considerationDepartment);
                            int rejectedDepartment;
                            resultDepartment.TryGetValue(2, out rejectedDepartment);

                            var firstColumnValuesDispatcherNIIARStatusDone = db.Applications.AsEnumerable().Where(x => (
                       (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID != 3) && x.DispatcherATA_StatusDoneID == 3) || (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID == 3) && x.DispatcherATA_StatusDoneID == 3).GroupBy(x => x.DispatcherNIIAR_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDispatcherNIIARStatusDone = firstColumnValuesDispatcherNIIARStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(1, out agreedDispatcherNIIARStatusDone);
                            int considerationDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(3, out considerationDispatcherNIIARStatusDone);
                            int rejectedDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(2, out rejectedDispatcherNIIARStatusDone);

                            var firstColumnValuesDispatcherATAStatusDone = db.Applications.AsEnumerable().Where(x => (
                       (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID != 3) && x.DispatcherATA_StatusDoneID == 3) || (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID == 3) && x.DispatcherATA_StatusDoneID == 3).GroupBy(x => x.DispatcherATA_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDispatcherATAStatusDone = firstColumnValuesDispatcherATAStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int ExecutedDispatcherATAStatusDone;
                            resultDispatcherATAStatusDone.TryGetValue(4, out ExecutedDispatcherATAStatusDone);
                            int NotExecutedDispatcherATAStatusDone;
                            resultDispatcherATAStatusDone.TryGetValue(5, out NotExecutedDispatcherATAStatusDone);
                            //посчитать количество определенных одинаковых данных
                            var firstColumnValuesSumm = db.Applications.AsEnumerable().Where(x => (
                       (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID != 3) && x.DispatcherATA_StatusDoneID == 3) || (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID == 3) && x.DispatcherATA_StatusDoneID == 3).Count();

                            ////минимальная/максимальная дата
                            var dateTimes = db.Applications.AsEnumerable().Where(x => (
                       (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID != 3) && x.DispatcherATA_StatusDoneID == 3) || (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
                       (x.DispatcherNIIAR_StatusDoneID != 3)
                        && (x.DepartmentStatusDoneID == 3) && x.DispatcherATA_StatusDoneID == 3).Select(x => x.DateCreation);
                            //var dateTimes = dgvApplicationAgreedView.Rows.Cast<DataGridViewRow>().Select(x => Convert.ToDateTime(x.Cells["dateCreationDataGridViewTextBoxColumn1"].Value));
                            var minValue = dateTimes.Min();
                            var maxValue = dateTimes.Max();
                            ////минимальная/максимальная дата



                            int[] number = Enumerable.Range(1, 35).ToArray();

                            for (int i = 1; i < number.Count() + 1; i++)
                            {
                                ExcelApp.Cells[21, i].Font.Bold = true;//жирный шрифт
                                ExcelApp.Cells[21, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                                ExcelApp.Cells[21, i].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            }


                            //цвет ячейки
                            ExcelApp.Cells[1, 1].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);
                            ExcelApp.Cells[1, 2].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);

                            ExcelApp.Cells[2, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[2, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[17, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[18, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[17, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[18, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[20, 1].Interior.Color = ColorTranslator.ToOle(Color.DarkGray);
                            //цвет ячейки


                            //строки вправо
                            for (int i = 1; i <= 18; i++)
                            {
                                ExcelApp.Cells[i, 2].HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            //строки вправо

                            ExcelApp.Cells[1, 2] = String.Format("{0:d} - {0:t}", minValue) + " — " + String.Format("{0:d} - {0:t}", maxValue);

                            ExcelApp.Cells[2, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[2, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[2, 1] = "Текущий статус";
                            ExcelApp.Cells[2, 2] = "Количество";
                            ExcelApp.Cells[3, 2] = considerationDirector;
                            ExcelApp.Cells[4, 2] = considerationEconomist;
                            ExcelApp.Cells[5, 2] = considerationDepartment;
                            ExcelApp.Cells[6, 2] = considerationDispatcherNIIARStatusDone;
                            ExcelApp.Cells[7, 2] = rejectedDirector;
                            ExcelApp.Cells[8, 2] = rejectedEconomist;
                            ExcelApp.Cells[9, 2] = rejectedDepartment;
                            ExcelApp.Cells[10, 2] = rejectedDispatcherNIIARStatusDone;
                            ExcelApp.Cells[11, 2] = agreedDirector;
                            ExcelApp.Cells[12, 2] = agreedEconomist;
                            ExcelApp.Cells[13, 2] = agreedDepartment;
                            ExcelApp.Cells[14, 2] = agreedDispatcherNIIARStatusDone;
                            ExcelApp.Cells[15, 2] = ExecutedDispatcherATAStatusDone;
                            ExcelApp.Cells[16, 2] = NotExecutedDispatcherATAStatusDone;
                            ExcelApp.Cells[17, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[18, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[17, 2] = firstColumnValuesSumm;
                            ExcelApp.Cells[18, 2] = String.Format("{0:d} - {0:t}", DateTime.Now);

                            ExcelApp.Cells[1, 1].Font.Bold = true;
                            ExcelApp.Cells[1, 2].Font.Bold = true;

                            //строки влево
                            for (int i = 1; i <= 18; i++)
                            {
                                ExcelApp.Cells[i, 1].HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            }
                            //строки влево
                            ExcelApp.Cells[1, 1].Font.Size = 16; //размер шрифта 
                            ExcelApp.Cells[1, 2].Font.Size = 16;
                            ExcelApp.Cells[20, 1] = "Заявки";
                            ExcelApp.Cells[20, 1].Font.Size = 16;
                            ExcelApp.Cells[20, 1].Font.Bold = true;
                            ExcelApp.Cells[1, 1] = "Отчёт по заявкам за период: ";
                            ExcelApp.Cells[3, 1] = "На рассмотрении у руководителей";
                            ExcelApp.Cells[4, 1] = "На рассмотрении у экономистов";
                            ExcelApp.Cells[5, 1] = "На рассмотрении у ДИД";
                            ExcelApp.Cells[6, 1] = "На рассмотрении у диспетчера НИИАР";
                            ExcelApp.Cells[7, 1] = "Отклонены руководителями";
                            ExcelApp.Cells[8, 1] = "Отклонены экономистами";
                            ExcelApp.Cells[9, 1] = "Отклонены ДИД";
                            ExcelApp.Cells[10, 1] = "Отклонены  диспетчером НИИАР";
                            ExcelApp.Cells[11, 1] = "Согласовано руководителями";
                            ExcelApp.Cells[12, 1] = "Согласовано экономистами";
                            ExcelApp.Cells[13, 1] = "Согласовано ДИД";
                            ExcelApp.Cells[14, 1] = "Согласовано диспетчером НИИАР";
                            ExcelApp.Cells[15, 1] = "Исполнено";
                            ExcelApp.Cells[16, 1] = "Не исполнено";
                            ExcelApp.Cells[17, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[17, 1] = "Количество поступивших заявок";
                            ExcelApp.Cells[18, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[18, 1] = "Дата оформления отчёта";



                        }

                    }
                    else
                    {

                        using (var db = new ASUZ_Transport_DBEntitie())
                        {
                            //посчитать количество определенных одинаковых данных
                            var firstColumnValuesDirector = db.Applications.AsEnumerable().Where(x => (x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && ((x.DispatcherNIIAR_StatusDoneID == 1 ||
                         x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2) || (DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) && (x.DirectorStatusDoneID == 3) || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3))).GroupBy(x => x.DirectorStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDirector = firstColumnValuesDirector.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDirector;
                            resultDirector.TryGetValue(1, out agreedDirector);
                            int considerationDirector;
                            resultDirector.TryGetValue(3, out considerationDirector);
                            int rejectedDirector;
                            resultDirector.TryGetValue(2, out rejectedDirector);


                            var firstColumnValuesEconomist = db.Applications.AsEnumerable().Where(x => (x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && ((x.DispatcherNIIAR_StatusDoneID == 1 ||
                         x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2) || (DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) && (x.DirectorStatusDoneID == 3) || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3))).GroupBy(x => x.EconomistStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultEconomist = firstColumnValuesEconomist.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedEconomist;
                            resultEconomist.TryGetValue(1, out agreedEconomist);
                            int considerationEconomist;
                            resultEconomist.TryGetValue(3, out considerationEconomist);
                            int rejectedEconomist;
                            resultEconomist.TryGetValue(2, out rejectedEconomist);

                            var firstColumnValuesDepartment = db.Applications.AsEnumerable().Where(x => ((x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && ((x.DispatcherNIIAR_StatusDoneID == 1 ||
                         x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2) || (DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) && (x.DirectorStatusDoneID == 3) || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3))) && (x.IntercityСity != true || x.Days != true)).GroupBy(x => x.DepartmentStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDepartment = firstColumnValuesDepartment.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDepartment;
                            resultDepartment.TryGetValue(1, out agreedDepartment);
                            int considerationDepartment;
                            resultDepartment.TryGetValue(3, out considerationDepartment);
                            int rejectedDepartment;
                            resultDepartment.TryGetValue(2, out rejectedDepartment);

                            var firstColumnValuesDispatcherNIIARStatusDone = db.Applications.AsEnumerable().Where(x => (x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && ((x.DispatcherNIIAR_StatusDoneID == 1 ||
                         x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2) || (DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) && (x.DirectorStatusDoneID == 3) || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3))).GroupBy(x => x.DispatcherNIIAR_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDispatcherNIIARStatusDone = firstColumnValuesDispatcherNIIARStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(1, out agreedDispatcherNIIARStatusDone);
                            int considerationDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(3, out considerationDispatcherNIIARStatusDone);
                            int rejectedDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(2, out rejectedDispatcherNIIARStatusDone);

                            var firstColumnValuesDispatcherATAStatusDone = db.Applications.AsEnumerable().Where(x => (x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && ((x.DispatcherNIIAR_StatusDoneID == 1 ||
                         x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2) || (DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) && (x.DirectorStatusDoneID == 3) || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3))).GroupBy(x => x.DispatcherATA_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDispatcherATAStatusDone = firstColumnValuesDispatcherATAStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int ExecutedDispatcherATAStatusDone;
                            resultDispatcherATAStatusDone.TryGetValue(4, out ExecutedDispatcherATAStatusDone);
                            int NotExecutedDispatcherATAStatusDone;
                            resultDispatcherATAStatusDone.TryGetValue(5, out NotExecutedDispatcherATAStatusDone);
                            //посчитать количество определенных одинаковых данных
                            var firstColumnValuesSumm = db.Applications.AsEnumerable().Where(x => (x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && ((x.DispatcherNIIAR_StatusDoneID == 1 ||
                         x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2) || (DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) && (x.DirectorStatusDoneID == 3) || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3))).Count();

                            ////минимальная/максимальная дата
                            var dateTimes = db.Applications.AsEnumerable().Where(x => (x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && ((x.DispatcherNIIAR_StatusDoneID == 1 ||
                         x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2) || (DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) && (x.DirectorStatusDoneID == 3) || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3))).Select(x => x.DateCreation);
                            //var dateTimes = dgvApplicationAgreedView.Rows.Cast<DataGridViewRow>().Select(x => Convert.ToDateTime(x.Cells["dateCreationDataGridViewTextBoxColumn1"].Value));
                            var minValue = dateTimes.Min();
                            var maxValue = dateTimes.Max();
                            ////минимальная/максимальная дата

                            int[] number = Enumerable.Range(1, 35).ToArray();

                            for (int i = 1; i < number.Count() + 1; i++)
                            {
                                ExcelApp.Cells[21, i].Font.Bold = true;//жирный шрифт
                                ExcelApp.Cells[21, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                                ExcelApp.Cells[21, i].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            }
                            //цвет ячейки
                            ExcelApp.Cells[1, 1].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);
                            ExcelApp.Cells[1, 2].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);

                            ExcelApp.Cells[2, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[2, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[17, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[18, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[17, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[18, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[20, 1].Interior.Color = ColorTranslator.ToOle(Color.DarkGray);
                            //цвет ячейки


                            //строки вправо
                            for (int i = 1; i <= 18; i++)
                            {
                                ExcelApp.Cells[i, 2].HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            //строки вправо

                            ExcelApp.Cells[1, 2] = String.Format("{0:d} - {0:t}", minValue) + " — " + String.Format("{0:d} - {0:t}", maxValue);

                            ExcelApp.Cells[2, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[2, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[2, 1] = "Текущий статус";
                            ExcelApp.Cells[2, 2] = "Количество";
                            ExcelApp.Cells[3, 2] = considerationDirector;
                            ExcelApp.Cells[4, 2] = considerationEconomist;
                            ExcelApp.Cells[5, 2] = considerationDepartment;
                            ExcelApp.Cells[6, 2] = considerationDispatcherNIIARStatusDone;
                            ExcelApp.Cells[7, 2] = rejectedDirector;
                            ExcelApp.Cells[8, 2] = rejectedEconomist;
                            ExcelApp.Cells[9, 2] = rejectedDepartment;
                            ExcelApp.Cells[10, 2] = rejectedDispatcherNIIARStatusDone;
                            ExcelApp.Cells[11, 2] = agreedDirector;
                            ExcelApp.Cells[12, 2] = agreedEconomist;
                            ExcelApp.Cells[13, 2] = agreedDepartment;
                            ExcelApp.Cells[14, 2] = agreedDispatcherNIIARStatusDone;
                            ExcelApp.Cells[15, 2] = ExecutedDispatcherATAStatusDone;
                            ExcelApp.Cells[16, 2] = NotExecutedDispatcherATAStatusDone;
                            ExcelApp.Cells[17, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[18, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[17, 2] = firstColumnValuesSumm;
                            ExcelApp.Cells[18, 2] = String.Format("{0:d} - {0:t}", DateTime.Now);

                            ExcelApp.Cells[1, 1].Font.Bold = true;
                            ExcelApp.Cells[1, 2].Font.Bold = true;

                            //строки влево
                            for (int i = 1; i <= 18; i++)
                            {
                                ExcelApp.Cells[i, 1].HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            }
                            //строки влево
                            ExcelApp.Cells[1, 1].Font.Size = 16; //размер шрифта 
                            ExcelApp.Cells[1, 2].Font.Size = 16;
                            ExcelApp.Cells[20, 1] = "Архив заявок";
                            ExcelApp.Cells[20, 1].Font.Size = 16;
                            ExcelApp.Cells[20, 1].Font.Bold = true;
                            ExcelApp.Cells[1, 1] = "Отчёт по заявкам за период: ";
                            ExcelApp.Cells[3, 1] = "На рассмотрении у руководителей";
                            ExcelApp.Cells[4, 1] = "На рассмотрении у экономистов";
                            ExcelApp.Cells[5, 1] = "На рассмотрении у ДИД";
                            ExcelApp.Cells[6, 1] = "На рассмотрении у диспетчера НИИАР";
                            ExcelApp.Cells[7, 1] = "Отклонены руководителями";
                            ExcelApp.Cells[8, 1] = "Отклонены экономистами";
                            ExcelApp.Cells[9, 1] = "Отклонены ДИД";
                            ExcelApp.Cells[10, 1] = "Отклонены  диспетчером НИИАР";
                            ExcelApp.Cells[11, 1] = "Согласовано руководителями";
                            ExcelApp.Cells[12, 1] = "Согласовано экономистами";
                            ExcelApp.Cells[13, 1] = "Согласовано ДИД";
                            ExcelApp.Cells[14, 1] = "Согласовано диспетчером НИИАР";
                            ExcelApp.Cells[15, 1] = "Исполнено";
                            ExcelApp.Cells[16, 1] = "Не исполнено";
                            ExcelApp.Cells[17, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[17, 1] = "Количество поступивших заявок";
                            ExcelApp.Cells[18, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[18, 1] = "Дата оформления отчёта";


                        }
                    }

                    using (var db = new ASUZ_Transport_DBEntitie())
                    {
                        List<Applications> applications = db.Applications.ToList();
                        MethodsRepository methodsRepository = new MethodsRepository();
                        applications = methodsRepository.GetApplications(archiveTrue, archiveFalse);

                        List<Applications> appL = new List<Applications>();

                        for (int i = 0; i < applications.Count(); i++)
                        {
                            foreach (var app in applications)
                            {
                                //    int[] appM = { app.Id };
                                //    appL.Add(app.Id);
                                appL.Add(app);

                            }

                            for (int ap = 0; ap < appL.Count; ap++)
                            {
                                ExcelApp.Cells[i + 22, 1] = appL[i].Id;
                                ExcelApp.Cells[i + 22, 2] = appL[i].Users.SurName + " " + appL[i].Users.Name + " " + appL[i].Users.Partonymic;
                                ExcelApp.Cells[i + 22, 3] = appL[i].Users.Email;
                                ExcelApp.Cells[i + 22, 4] = appL[i].Users.Post;
                                ExcelApp.Cells[i + 22, 5] = appL[i].Users.Divisions.Name;


                                foreach (var usr in appL.Where(x => x.Users.Id == appL[i].UserID))
                                {
                                    foreach (var usrDrc in db.Users.Where(x => x.DivisionID == usr.Users.DivisionID && x.StatusID == 3))
                                    {
                                        ExcelApp.Cells[i + 22, 6] = usrDrc.SurName + " " + usrDrc.Name + " " + usrDrc.Partonymic;
                                    }
                                }
                                foreach (var usr in appL.Where(x => x.Users.Id == appL[i].UserID))
                                {
                                    foreach (var usrEcn in db.Users.Where(x => x.DivisionID == usr.Users.DivisionID && x.StatusID == 4))
                                    {

                                        ExcelApp.Cells[i + 22, 7] = usrEcn.SurName + " " + usrEcn.Name + " " + usrEcn.Partonymic;
                                    }
                                }
                                ExcelApp.Cells[i + 22, 8] = appL[i].CPC;
                                ExcelApp.Cells[i + 22, 9] = string.Format("{0}", appL[i].IntercityСity.Value ? "Город" : "Межгород");
                                ExcelApp.Cells[i + 22, 10] = appL[i].PurposeUsingTransport;
                                ExcelApp.Cells[i + 22, 11] = string.Format("{0}", appL[i].Days.Value ? "Рабочий" : "Выходной");
                                ExcelApp.Cells[i + 22, 12] = appL[i].Users.WorkPhone;
                                ExcelApp.Cells[i + 22, 13] = appL[i].Users.MobilePhone;
                                ExcelApp.Cells[i + 22, 14] = appL[i].DateCreation;
                                ExcelApp.Cells[i + 22, 15] = appL[i].StartDate;
                                ExcelApp.Cells[i + 22, 16] = appL[i].EndDate;
                                ExcelApp.Cells[i + 22, 17] = appL[i].TypeCars.Name;
                                ExcelApp.Cells[i + 22, 18] = appL[i].Cars.ModelCars.Name;
                                ExcelApp.Cells[i + 22, 19] = appL[i].Cars.RegisterSign;
                                ExcelApp.Cells[i + 22, 20] = appL[i].QuantityPassengers;
                                ExcelApp.Cells[i + 22, 21] = appL[i].CargoWeight;
                                ExcelApp.Cells[i + 22, 22] = appL[i].PlaceSubmission;
                                ExcelApp.Cells[i + 22, 23] = appL[i].Route;
                                ExcelApp.Cells[i + 22, 24] = appL[i].CommentClient;
                                ExcelApp.Cells[i + 22, 25] = appL[i].СommentDirector;
                                ExcelApp.Cells[i + 22, 26] = appL[i].СommentEconomist;
                                ExcelApp.Cells[i + 22, 27] = appL[i].СommentDepartment;
                                ExcelApp.Cells[i + 22, 28] = appL[i].СommentDispatcherNIIAR;
                                ExcelApp.Cells[i + 22, 29] = appL[i].СommentDispatcherATA;
                                ExcelApp.Cells[i + 22, 30] = appL[i].StatusesDone.Name;
                                ExcelApp.Cells[i + 22, 31] = appL[i].StatusesDone1.Name;
                                ExcelApp.Cells[i + 22, 32] = appL[i].StatusesDone3.Name;
                                ExcelApp.Cells[i + 22, 33] = appL[i].StatusesDone2.Name;
                                ExcelApp.Cells[i + 22, 34] = appL[i].StatusesDone4.Name;
                                ExcelApp.Cells[i + 22, 35] = appL[i].ApplicationJoin;
                            }

                        }
                    }
                }
                if (Users.mode == (int)Users.Status.Admin)
                {
                    if (archiveTrue == true)
                    {
                        using (var db = new ASUZ_Transport_DBEntitie())
                        {
                            //посчитать количество определенных одинаковых данных
                            var firstColumnValuesDirector = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) < new TimeSpan(24, 0, 0, 0) &&
                         ((x.DirectorStatusDoneID == 3 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3))).GroupBy(x => x.DirectorStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDirector = firstColumnValuesDirector.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDirector;
                            resultDirector.TryGetValue(1, out agreedDirector);
                            int considerationDirector;
                            resultDirector.TryGetValue(3, out considerationDirector);
                            int rejectedDirector;
                            resultDirector.TryGetValue(2, out rejectedDirector);


                            var firstColumnValuesEconomist = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) < new TimeSpan(24, 0, 0, 0) &&
                         ((x.DirectorStatusDoneID == 3 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3))).GroupBy(x => x.EconomistStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultEconomist = firstColumnValuesEconomist.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedEconomist;
                            resultEconomist.TryGetValue(1, out agreedEconomist);
                            int considerationEconomist;
                            resultEconomist.TryGetValue(3, out considerationEconomist);
                            int rejectedEconomist;
                            resultEconomist.TryGetValue(2, out rejectedEconomist);

                            var firstColumnValuesDepartment = db.Applications.AsEnumerable().Where(x => (DateTime.Now - x.DateCreation.AddDays(-9) < new TimeSpan(24, 0, 0, 0) &&
                         ((x.DirectorStatusDoneID == 3 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3))) && (x.IntercityСity != true || x.Days != true)).GroupBy(x => x.DepartmentStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDepartment = firstColumnValuesDepartment.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDepartment;
                            resultDepartment.TryGetValue(1, out agreedDepartment);
                            int considerationDepartment;
                            resultDepartment.TryGetValue(3, out considerationDepartment);
                            int rejectedDepartment;
                            resultDepartment.TryGetValue(2, out rejectedDepartment);

                            var firstColumnValuesDispatcherNIIARStatusDone = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) < new TimeSpan(24, 0, 0, 0) &&
                         ((x.DirectorStatusDoneID == 3 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3))).GroupBy(x => x.DispatcherNIIAR_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDispatcherNIIARStatusDone = firstColumnValuesDispatcherNIIARStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(1, out agreedDispatcherNIIARStatusDone);
                            int considerationDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(3, out considerationDispatcherNIIARStatusDone);
                            int rejectedDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(2, out rejectedDispatcherNIIARStatusDone);

                            var firstColumnValuesDispatcherATAStatusDone = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) < new TimeSpan(24, 0, 0, 0) &&
                         ((x.DirectorStatusDoneID == 3 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3))).GroupBy(x => x.DispatcherATA_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDispatcherATAStatusDone = firstColumnValuesDispatcherATAStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int ExecutedDispatcherATAStatusDone;
                            resultDispatcherATAStatusDone.TryGetValue(4, out ExecutedDispatcherATAStatusDone);
                            int NotExecutedDispatcherATAStatusDone;
                            resultDispatcherATAStatusDone.TryGetValue(5, out NotExecutedDispatcherATAStatusDone);
                            //посчитать количество определенных одинаковых данных
                            var firstColumnValuesSumm = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) < new TimeSpan(24, 0, 0, 0) &&
                         ((x.DirectorStatusDoneID == 3 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3))).Count();

                            ////минимальная/максимальная дата
                            var dateTimes = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) < new TimeSpan(24, 0, 0, 0) &&
                         ((x.DirectorStatusDoneID == 3 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3) ||
                         (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3))).Select(x => x.DateCreation);
                            //var dateTimes = dgvApplicationAgreedView.Rows.Cast<DataGridViewRow>().Select(x => Convert.ToDateTime(x.Cells["dateCreationDataGridViewTextBoxColumn1"].Value));
                            var minValue = dateTimes.Min();
                            var maxValue = dateTimes.Max();
                            ////минимальная/максимальная дата


                            int[] number = Enumerable.Range(1, 35).ToArray();

                            for (int i = 1; i < number.Count() + 1; i++)
                            {
                                ExcelApp.Cells[21, i].Font.Bold = true;//жирный шрифт
                                ExcelApp.Cells[21, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                                ExcelApp.Cells[21, i].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            }
                            //цвет ячейки
                            ExcelApp.Cells[1, 1].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);
                            ExcelApp.Cells[1, 2].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);

                            ExcelApp.Cells[2, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[2, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[17, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[18, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[17, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[18, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[20, 1].Interior.Color = ColorTranslator.ToOle(Color.DarkGray);
                            //цвет ячейки


                            //строки вправо
                            for (int i = 1; i <= 18; i++)
                            {
                                ExcelApp.Cells[i, 2].HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            //строки вправо

                            ExcelApp.Cells[1, 2] = String.Format("{0:d} - {0:t}", minValue) + " — " + String.Format("{0:d} - {0:t}", maxValue);

                            ExcelApp.Cells[2, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[2, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[2, 1] = "Текущий статус";
                            ExcelApp.Cells[2, 2] = "Количество";
                            ExcelApp.Cells[3, 2] = considerationDirector;
                            ExcelApp.Cells[4, 2] = considerationEconomist;
                            ExcelApp.Cells[5, 2] = considerationDepartment;
                            ExcelApp.Cells[6, 2] = considerationDispatcherNIIARStatusDone;
                            ExcelApp.Cells[7, 2] = rejectedDirector;
                            ExcelApp.Cells[8, 2] = rejectedEconomist;
                            ExcelApp.Cells[9, 2] = rejectedDepartment;
                            ExcelApp.Cells[10, 2] = rejectedDispatcherNIIARStatusDone;
                            ExcelApp.Cells[11, 2] = agreedDirector;
                            ExcelApp.Cells[12, 2] = agreedEconomist;
                            ExcelApp.Cells[13, 2] = agreedDepartment;
                            ExcelApp.Cells[14, 2] = agreedDispatcherNIIARStatusDone;
                            ExcelApp.Cells[15, 2] = ExecutedDispatcherATAStatusDone;
                            ExcelApp.Cells[16, 2] = NotExecutedDispatcherATAStatusDone;
                            ExcelApp.Cells[17, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[18, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[17, 2] = firstColumnValuesSumm;
                            ExcelApp.Cells[18, 2] = String.Format("{0:d} - {0:t}", DateTime.Now);

                            ExcelApp.Cells[1, 1].Font.Bold = true;
                            ExcelApp.Cells[1, 2].Font.Bold = true;

                            //строки влево
                            for (int i = 1; i <= 18; i++)
                            {
                                ExcelApp.Cells[i, 1].HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            }
                            //строки влево
                            ExcelApp.Cells[1, 1].Font.Size = 16; //размер шрифта 
                            ExcelApp.Cells[1, 2].Font.Size = 16;
                            ExcelApp.Cells[20, 1] = "Заявки";
                            ExcelApp.Cells[20, 1].Font.Size = 16;
                            ExcelApp.Cells[20, 1].Font.Bold = true;
                            ExcelApp.Cells[1, 1] = "Отчёт по заявкам за период: ";
                            ExcelApp.Cells[3, 1] = "На рассмотрении у руководителей";
                            ExcelApp.Cells[4, 1] = "На рассмотрении у экономистов";
                            ExcelApp.Cells[5, 1] = "На рассмотрении у ДИД";
                            ExcelApp.Cells[6, 1] = "На рассмотрении у диспетчера НИИАР";
                            ExcelApp.Cells[7, 1] = "Отклонены руководителями";
                            ExcelApp.Cells[8, 1] = "Отклонены экономистами";
                            ExcelApp.Cells[9, 1] = "Отклонены ДИД";
                            ExcelApp.Cells[10, 1] = "Отклонены  диспетчером НИИАР";
                            ExcelApp.Cells[11, 1] = "Согласовано руководителями";
                            ExcelApp.Cells[12, 1] = "Согласовано экономистами";
                            ExcelApp.Cells[13, 1] = "Согласовано ДИД";
                            ExcelApp.Cells[14, 1] = "Согласовано диспетчером НИИАР";
                            ExcelApp.Cells[15, 1] = "Исполнено";
                            ExcelApp.Cells[16, 1] = "Не исполнено";
                            ExcelApp.Cells[17, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[17, 1] = "Количество поступивших заявок";
                            ExcelApp.Cells[18, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[18, 1] = "Дата оформления отчёта";


                        }
                    }
                    else
                    {
                        using (var db = new ASUZ_Transport_DBEntitie())
                        {
                            //посчитать количество определенных одинаковых данных
                            var firstColumnValuesDirector = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) || ((x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && (x.DispatcherNIIAR_StatusDoneID == 1 ||
                          x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2))).GroupBy(x => x.DirectorStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDirector = firstColumnValuesDirector.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDirector;
                            resultDirector.TryGetValue(1, out agreedDirector);
                            int considerationDirector;
                            resultDirector.TryGetValue(3, out considerationDirector);
                            int rejectedDirector;
                            resultDirector.TryGetValue(2, out rejectedDirector);


                            var firstColumnValuesEconomist = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) || ((x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && (x.DispatcherNIIAR_StatusDoneID == 1 ||
                          x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2))).GroupBy(x => x.EconomistStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultEconomist = firstColumnValuesEconomist.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedEconomist;
                            resultEconomist.TryGetValue(1, out agreedEconomist);
                            int considerationEconomist;
                            resultEconomist.TryGetValue(3, out considerationEconomist);
                            int rejectedEconomist;
                            resultEconomist.TryGetValue(2, out rejectedEconomist);

                            var firstColumnValuesDepartment = db.Applications.AsEnumerable().Where(x => (DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) || ((x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && (x.DispatcherNIIAR_StatusDoneID == 1 ||
                          x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2))) && (x.IntercityСity != true || x.Days != true)).GroupBy(x => x.DepartmentStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDepartment = firstColumnValuesDepartment.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDepartment;
                            resultDepartment.TryGetValue(1, out agreedDepartment);
                            int considerationDepartment;
                            resultDepartment.TryGetValue(3, out considerationDepartment);
                            int rejectedDepartment;
                            resultDepartment.TryGetValue(2, out rejectedDepartment);

                            var firstColumnValuesDispatcherNIIARStatusDone = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) || ((x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && (x.DispatcherNIIAR_StatusDoneID == 1 ||
                          x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2))).GroupBy(x => x.DispatcherNIIAR_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDispatcherNIIARStatusDone = firstColumnValuesDispatcherNIIARStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int agreedDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(1, out agreedDispatcherNIIARStatusDone);
                            int considerationDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(3, out considerationDispatcherNIIARStatusDone);
                            int rejectedDispatcherNIIARStatusDone;
                            resultDispatcherNIIARStatusDone.TryGetValue(2, out rejectedDispatcherNIIARStatusDone);

                            var firstColumnValuesDispatcherATAStatusDone = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) || ((x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && (x.DispatcherNIIAR_StatusDoneID == 1 ||
                          x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2))).GroupBy(x => x.DispatcherATA_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                            Dictionary<int, int> resultDispatcherATAStatusDone = firstColumnValuesDispatcherATAStatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                            int ExecutedDispatcherATAStatusDone;
                            resultDispatcherATAStatusDone.TryGetValue(4, out ExecutedDispatcherATAStatusDone);
                            int NotExecutedDispatcherATAStatusDone;
                            resultDispatcherATAStatusDone.TryGetValue(5, out NotExecutedDispatcherATAStatusDone);
                            //посчитать количество определенных одинаковых данных
                            var firstColumnValuesSumm = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) || ((x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && (x.DispatcherNIIAR_StatusDoneID == 1 ||
                          x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2))).Count();

                            ////минимальная/максимальная дата
                            var dateTimes = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation.AddDays(-9) > new TimeSpan(24, 0, 0, 0) || ((x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && (x.DispatcherNIIAR_StatusDoneID == 1 ||
                          x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2))).Select(x => x.DateCreation);
                            //var dateTimes = dgvApplicationAgreedView.Rows.Cast<DataGridViewRow>().Select(x => Convert.ToDateTime(x.Cells["dateCreationDataGridViewTextBoxColumn1"].Value));
                            var minValue = dateTimes.Min();
                            var maxValue = dateTimes.Max();
                            ////минимальная/максимальная дата


                            int[] number = Enumerable.Range(1, 35).ToArray();

                            for (int i = 1; i < number.Count() + 1; i++)
                            {
                                ExcelApp.Cells[21, i].Font.Bold = true;//жирный шрифт
                                ExcelApp.Cells[21, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                                ExcelApp.Cells[21, i].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            }
                            //цвет ячейки
                            ExcelApp.Cells[1, 1].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);
                            ExcelApp.Cells[1, 2].Interior.Color = ColorTranslator.ToOle(Color.Gainsboro);

                            ExcelApp.Cells[2, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[2, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[17, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[18, 1].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[17, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[18, 2].Interior.Color = ColorTranslator.ToOle(Color.Silver);
                            ExcelApp.Cells[20, 1].Interior.Color = ColorTranslator.ToOle(Color.DarkGray);
                            //цвет ячейки


                            //строки вправо
                            for (int i = 1; i <= 18; i++)
                            {
                                ExcelApp.Cells[i, 2].HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            //строки вправо

                            ExcelApp.Cells[1, 2] = String.Format("{0:d} - {0:t}", minValue) + " — " + String.Format("{0:d} - {0:t}", maxValue);

                            ExcelApp.Cells[2, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[2, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[2, 1] = "Текущий статус";
                            ExcelApp.Cells[2, 2] = "Количество";
                            ExcelApp.Cells[3, 2] = considerationDirector;
                            ExcelApp.Cells[4, 2] = considerationEconomist;
                            ExcelApp.Cells[5, 2] = considerationDepartment;
                            ExcelApp.Cells[6, 2] = considerationDispatcherNIIARStatusDone;
                            ExcelApp.Cells[7, 2] = rejectedDirector;
                            ExcelApp.Cells[8, 2] = rejectedEconomist;
                            ExcelApp.Cells[9, 2] = rejectedDepartment;
                            ExcelApp.Cells[10, 2] = rejectedDispatcherNIIARStatusDone;
                            ExcelApp.Cells[11, 2] = agreedDirector;
                            ExcelApp.Cells[12, 2] = agreedEconomist;
                            ExcelApp.Cells[13, 2] = agreedDepartment;
                            ExcelApp.Cells[14, 2] = agreedDispatcherNIIARStatusDone;
                            ExcelApp.Cells[15, 2] = ExecutedDispatcherATAStatusDone;
                            ExcelApp.Cells[16, 2] = NotExecutedDispatcherATAStatusDone;
                            ExcelApp.Cells[17, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[18, 2].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[17, 2] = firstColumnValuesSumm;
                            ExcelApp.Cells[18, 2] = String.Format("{0:d} - {0:t}", DateTime.Now);

                            ExcelApp.Cells[1, 1].Font.Bold = true;
                            ExcelApp.Cells[1, 2].Font.Bold = true;

                            //строки влево
                            for (int i = 1; i <= 18; i++)
                            {
                                ExcelApp.Cells[i, 1].HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            }
                            //строки влево
                            ExcelApp.Cells[1, 1].Font.Size = 16; //размер шрифта 
                            ExcelApp.Cells[1, 2].Font.Size = 16;
                            ExcelApp.Cells[20, 1] = "Архив заявок";
                            ExcelApp.Cells[20, 1].Font.Size = 16;
                            ExcelApp.Cells[20, 1].Font.Bold = true;
                            ExcelApp.Cells[1, 1] = "Отчёт по заявкам за период: ";
                            ExcelApp.Cells[3, 1] = "На рассмотрении у руководителей";
                            ExcelApp.Cells[4, 1] = "На рассмотрении у экономистов";
                            ExcelApp.Cells[5, 1] = "На рассмотрении у ДИД";
                            ExcelApp.Cells[6, 1] = "На рассмотрении у диспетчера НИИАР";
                            ExcelApp.Cells[7, 1] = "Отклонены руководителями";
                            ExcelApp.Cells[8, 1] = "Отклонены экономистами";
                            ExcelApp.Cells[9, 1] = "Отклонены ДИД";
                            ExcelApp.Cells[10, 1] = "Отклонены  диспетчером НИИАР";
                            ExcelApp.Cells[11, 1] = "Согласовано руководителями";
                            ExcelApp.Cells[12, 1] = "Согласовано экономистами";
                            ExcelApp.Cells[13, 1] = "Согласовано ДИД";
                            ExcelApp.Cells[14, 1] = "Согласовано диспетчером НИИАР";
                            ExcelApp.Cells[15, 1] = "Исполнено";
                            ExcelApp.Cells[16, 1] = "Не исполнено";
                            ExcelApp.Cells[17, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[17, 1] = "Количество поступивших заявок";
                            ExcelApp.Cells[18, 1].Font.Bold = true;//жирный шрифт
                            ExcelApp.Cells[18, 1] = "Дата оформления отчёта";


                        }
                    }

                    using (var db = new ASUZ_Transport_DBEntitie())
                    {
                        List<Applications> applications = db.Applications.ToList();
                        MethodsRepository methodsRepository = new MethodsRepository();
                        applications = methodsRepository.GetApplications(archiveTrue, archiveFalse);

                        List<Applications> appL = new List<Applications>();

                        for (int i = 0; i < applications.Count(); i++)
                        {
                            foreach (var app in applications)
                            {
                                //    int[] appM = { app.Id };
                                //    appL.Add(app.Id);
                                appL.Add(app);

                            }

                            for (int ap = 0; ap < appL.Count; ap++)
                            {
                                ExcelApp.Cells[i + 22, 1] = appL[i].Id;
                                ExcelApp.Cells[i + 22, 2] = appL[i].Users.SurName + " " + appL[i].Users.Name + " " + appL[i].Users.Partonymic;
                                ExcelApp.Cells[i + 22, 3] = appL[i].Users.Email;
                                ExcelApp.Cells[i + 22, 4] = appL[i].Users.Post;
                                ExcelApp.Cells[i + 22, 5] = appL[i].Users.Divisions.Name;


                                foreach (var usr in appL.Where(x => x.Users.Id == appL[i].UserID))
                                {
                                    foreach (var usrDrc in db.Users.Where(x => x.DivisionID == usr.Users.DivisionID && x.StatusID == 3))
                                    {
                                        ExcelApp.Cells[i + 22, 6] = usrDrc.SurName + " " + usrDrc.Name + " " + usrDrc.Partonymic;
                                    }
                                }
                                foreach (var usr in appL.Where(x => x.Users.Id == appL[i].UserID))
                                {
                                    foreach (var usrEcn in db.Users.Where(x => x.DivisionID == usr.Users.DivisionID && x.StatusID == 4))
                                    {

                                        ExcelApp.Cells[i + 22, 7] = usrEcn.SurName + " " + usrEcn.Name + " " + usrEcn.Partonymic;
                                    }
                                }
                                ExcelApp.Cells[i + 22, 8] = appL[i].CPC;
                                ExcelApp.Cells[i + 22, 9] = string.Format("{0}", appL[i].IntercityСity.Value ? "Город" : "Межгород");
                                ExcelApp.Cells[i + 22, 10] = appL[i].PurposeUsingTransport;
                                ExcelApp.Cells[i + 22, 11] = string.Format("{0}", appL[i].Days.Value ? "Рабочий" : "Выходной");
                                ExcelApp.Cells[i + 22, 12] = appL[i].Users.WorkPhone;
                                ExcelApp.Cells[i + 22, 13] = appL[i].Users.MobilePhone;
                                ExcelApp.Cells[i + 22, 14] = appL[i].DateCreation;
                                ExcelApp.Cells[i + 22, 15] = appL[i].StartDate;
                                ExcelApp.Cells[i + 22, 16] = appL[i].EndDate;
                                ExcelApp.Cells[i + 22, 17] = appL[i].TypeCars.Name;
                                ExcelApp.Cells[i + 22, 18] = appL[i].Cars.ModelCars.Name;
                                ExcelApp.Cells[i + 22, 19] = appL[i].Cars.RegisterSign;
                                ExcelApp.Cells[i + 22, 20] = appL[i].QuantityPassengers;
                                ExcelApp.Cells[i + 22, 21] = appL[i].CargoWeight;
                                ExcelApp.Cells[i + 22, 22] = appL[i].PlaceSubmission;
                                ExcelApp.Cells[i + 22, 23] = appL[i].Route;
                                ExcelApp.Cells[i + 22, 24] = appL[i].CommentClient;
                                ExcelApp.Cells[i + 22, 25] = appL[i].СommentDirector;
                                ExcelApp.Cells[i + 22, 26] = appL[i].СommentEconomist;
                                ExcelApp.Cells[i + 22, 27] = appL[i].СommentDepartment;
                                ExcelApp.Cells[i + 22, 28] = appL[i].СommentDispatcherNIIAR;
                                ExcelApp.Cells[i + 22, 29] = appL[i].СommentDispatcherATA;
                                ExcelApp.Cells[i + 22, 30] = appL[i].StatusesDone.Name;
                                ExcelApp.Cells[i + 22, 31] = appL[i].StatusesDone1.Name;
                                ExcelApp.Cells[i + 22, 32] = appL[i].StatusesDone3.Name;
                                ExcelApp.Cells[i + 22, 33] = appL[i].StatusesDone2.Name;
                                ExcelApp.Cells[i + 22, 34] = appL[i].StatusesDone4.Name;
                                ExcelApp.Cells[i + 22, 35] = appL[i].ApplicationJoin;
                            }

                        }
                    }


                }




                //название столбцов
                ExcelApp.Cells[21, 1] = "№";
                ExcelApp.Cells[21, 2] = "Клиент";
                ExcelApp.Cells[21, 3] = "Email";
                ExcelApp.Cells[21, 4] = "Должность";
                ExcelApp.Cells[21, 5] = "Подразделение";
                ExcelApp.Cells[21, 6] = "Руководитель";
                ExcelApp.Cells[21, 7] = "Экономист";
                ExcelApp.Cells[21, 8] = "ШПЗ";
                ExcelApp.Cells[21, 9] = "Межгород/Город";
                ExcelApp.Cells[21, 10] = "Цель использования транспорта";
                ExcelApp.Cells[21, 11] = "День";
                ExcelApp.Cells[21, 12] = "Рабочий телефон";
                ExcelApp.Cells[21, 13] = "Мобильный телефон";
                ExcelApp.Cells[21, 14] = "Дата создания";
                ExcelApp.Cells[21, 15] = "Начало работы";
                ExcelApp.Cells[21, 16] = "Завершение работы";
                ExcelApp.Cells[21, 17] = "Тип транспорта";
                ExcelApp.Cells[21, 18] = "Модель";
                ExcelApp.Cells[21, 19] = "Гос. номер";
                ExcelApp.Cells[21, 20] = "Количество пассажиров";
                ExcelApp.Cells[21, 21] = "Груз (кг)";
                ExcelApp.Cells[21, 22] = "Место подачи";
                ExcelApp.Cells[21, 23] = "Маршрут";
                ExcelApp.Cells[21, 24] = "Комментарий клиента";
                ExcelApp.Cells[21, 25] = "Комментарий руководителя";
                ExcelApp.Cells[21, 26] = "Комментарий экономиста";
                ExcelApp.Cells[21, 27] = "Комментарий ДИД";
                ExcelApp.Cells[21, 28] = "Комментарий диспетчера НИИАР";
                ExcelApp.Cells[21, 29] = "Комментарий диспетчера АТА";
                ExcelApp.Cells[21, 30] = "Статус у руководителя";
                ExcelApp.Cells[21, 31] = "Статус у экономиста";
                ExcelApp.Cells[21, 32] = "Статус у ДИД";
                ExcelApp.Cells[21, 33] = "Статус у диспетчера НИИАР";
                ExcelApp.Cells[21, 34] = "Статус у диспетчера АТА";
                ExcelApp.Cells[21, 35] = "Объединенные заявки";
                //название столбцов

                ExcelApp.Columns.AutoFit();//автоширина столбцов(ВЫПОЛНЕНИЕ ДОЛЖНО БЫТЬ ПОСЛЕ ЗАПОЛНЕНИЯ СТОЛБЦОВ ДАННЫМИ!)
                                           //удаление файла, если он есть в папке
                FileInfo fileInf = new FileInfo(Server.MapPath(@"~/Content/Excel/" + Users.UserID + ".xlsx"));
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                    // альтернатива с помощью класса File
                    // File.Delete(path);
                }
                //удаление файла, если он есть в папке
                //сохранение файла
                ExcelWorkBook.SaveAs(Server.MapPath(@"~/Content/Excel/" + Users.UserID + ".xlsx"));
                //сохранение файла

                //остановить процесс
                ExcelWorkBook.Close();
                ExcelApp.Quit();
                //остановить процесс

                // Путь к файлу
                string file_path = Server.MapPath(@"~/Content/Excel/" + Users.UserID + ".xlsx");
                // Тип файла - content-type
                string file_type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                if (archiveTrue == true)
                {
                    // Имя файла - необязательно
                    string file_name = "Заявки_" + String.Format("{0:d}", DateTime.Now) + ".xlsx";
                    //загрузка сформированного файла
                    return File(file_path, file_type, file_name);
                    //загрузка сформированного файла
                }
                else
                {
                    string file_name = "Архив заявок_" + String.Format("{0:d}", DateTime.Now) + ".xlsx";
                    return File(file_path, file_type, file_name);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return null;
        }

        //public ActionResult Index(FilteringApplications filteringApplications, bool archiveTrue = true, bool archiveFalse = false, int id = 0)
        //{
        //    ASUZ_Transport_DBEntities db = new ASUZ_Transport_DBEntities();
        //    List<Applications> applications = db.Applications.ToList();
        //    List<Users> users = db.Users.ToList();
        //    MethodsRepository methodsRepository = new MethodsRepository();
        //    applications = methodsRepository.GetApplications(archiveTrue, archiveFalse);
        //    ViewBag.archiveTrue = archiveTrue;
        //    ViewBag.archiveFalse = archiveFalse;
        //    ViewBag.UserID = Users.UserID;
        //    ViewBag.statusID = Users.mode.ToString();
        //    ViewBag.applications = applications.Count();
        //    FilteringApplications aplc = new FilteringApplications
        //    {
        //        Applications = applications.ToList(),
        //        Users = users.ToList(),
        //        //archiveApplications = false
        //        //Director = new SelectList(usrDrc, "Id", "SurName")
        //    };

        //    return View(aplc);
        //}


        //[HttpPost]
        //public ActionResult Index(FilteringApplications filteringApplications, bool archiveTrue, bool archiveFalse)
        //{
        //    ASUZ_Transport_DBEntities db = new ASUZ_Transport_DBEntities();
        //    List<Applications> applications = db.Applications.ToList();
        //    List<Users> users = db.Users.ToList();
        //    MethodsRepository methodsRepository = new MethodsRepository();
        //    ViewBag.archiveTrue = archiveTrue;
        //    ViewBag.archiveFalse = archiveFalse;
        //    ViewBag.UserID = Users.UserID;
        //    ViewBag.statusID = Users.mode.ToString();
        //    ViewBag.applications = applications.Count();
        //    applications = methodsRepository.GetApplications(archiveTrue, archiveFalse);

        //    FilteringApplications aplc = new FilteringApplications
        //    {
        //        Applications = applications.ToList(),
        //        Users = users.ToList()
        //        //Director = new SelectList(usrDrc, "Id", "SurName")
        //    };
        //    return View(aplc);
        //}


        public ActionResult Index(FilteringApplications filteringApplications, bool archiveTrue = true, bool archiveFalse = false, int id = 0)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            List<Applications> applications = db.Applications.ToList();
            List<Users> users = db.Users.ToList();
            MethodsRepository methodsRepository = new MethodsRepository();
            ViewBag.archiveTrue = archiveTrue;
            ViewBag.archiveFalse = archiveFalse;
            ViewBag.UserID = Users.UserID;
            ViewBag.statusID = Users.mode.ToString();
            ViewBag.applications = applications.Count();

            applications = methodsRepository.GetApplications(archiveTrue, archiveFalse);

            FilteringApplications aplc = new FilteringApplications
            {
                Applications = applications.ToList(),
                Users = users.ToList()

            };
            _MaxId = 0;
            return View(aplc);
        }


        [HttpPost]
        public ActionResult Index(FilteringApplications filteringApplications, bool archiveTrue, bool archiveFalse)
        {
            HomeController home = new HomeController();
            home.Index(filteringApplications, archiveTrue, archiveFalse);
            _MaxId = 0;
            return View(home);
        }


        [HttpPost]
        public ActionResult _IndexPartial(FilteringApplications filteringApplications, bool archiveTrue = true, bool archiveFalse = false)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            List<Applications> applications = db.Applications.ToList();
            List<Users> users = db.Users.ToList();
            MethodsRepository methodsRepository = new MethodsRepository();
            //archiveTrue = filteringApplications.archiveTrue;
            //archiveFalse = filteringApplications.archiveFalse;
            ViewBag.archiveTrue = archiveTrue;
            ViewBag.archiveFalse = archiveFalse;
            //archiveTrue = Convert.ToBoolean(ViewBag.archiveTrue == "" ? "true" : ViewBag.archiveTrue);
            //archiveFalse = Convert.ToBoolean(ViewBag.archiveFalse == "" ? "false" : ViewBag.archiveFalse);
            ViewBag.UserID = Users.UserID;
            ViewBag.statusID = Users.mode.ToString();
            ViewBag.applications = applications.Count();
            applications = methodsRepository.GetApplications(archiveTrue, archiveFalse);

            FilteringApplications aplc = new FilteringApplications
            {
                Applications = applications.ToList(),
                Users = users.ToList()
            };
            //if (applications.Count <= 0)
            //{
            //    return HttpNotFound();
            //}


            if (_MaxId < methodsRepository.CheckMaxId())
            {
                _MaxId = methodsRepository.CheckMaxId();
                //ViewBag.message = "Поступила новая заявка № " + methodsRepository.CheckMaxId();
                //ModelState.AddModelError("", "Поступила новая заявка № " + methodsRepository.CheckMaxId());
                return PartialView(aplc);
            }
            return HttpNotFound();
            //return RedirectToAction("");
        }



        //принять заявку 
        public ActionResult Accept(int id)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            Applications applications = new Applications();
            ViewBag.statusID = Users.mode.ToString();
            applications = db.Applications.Find(id);
            if (applications != null)
            {
                return PartialView("Accept", applications);
            }
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Accept(Applications applications, FilteringApplications filteringApplications/*, int Id = 0, int drcAccept = 0, int drcReject = 0*/)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();

            //db.Entry(applications).State = EntityState.Modified; // для всего объекта сразу
            MethodsRepository app = new MethodsRepository();
            //if (applications.CPC != null)
            //{
            app.SaveApplication(applications, filteringApplications);
            _MaxId = 0;
            return RedirectToAction("Index");
            //}
            //else
            //{
            //    Applications application = new Applications();
            //    application = db.Applications.Find(applications.Id);
            //    return PartialView("Accept", application);
            //}

            //ASUZ_Transport_DBEntities db = new ASUZ_Transport_DBEntities();
            ////Applications application = null;
            //applications = db.Applications.FirstOrDefault(b => b.Id == Id);

            //MethodsRepository pr = new MethodsRepository();
            //if (drcAccept != 0)
            //{
            //    TempData["message"] = string.Format("Вы приняли заявку № \"{0}\"!", applications.Id);
            //}
            //if (drcReject != 0)
            //{
            //    TempData["message"] = string.Format("Вы отклонили заявку № \"{0}\"!", applications.Id);
            //}
            //pr.SaveApplication(applications, drcAccept, drcReject);
            //return RedirectToAction("Index");
        }
        //принять заявку 

        //отклонить заявку 
        public ActionResult Reject(int id)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            Applications applications = new Applications();
            ViewBag.statusID = Users.mode.ToString();
            applications = db.Applications.Find(id);
            if (applications != null)
            {
                return PartialView("Reject", applications);
            }
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(Applications applications, FilteringApplications filteringApplications)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();

            MethodsRepository app = new MethodsRepository();
            app.SaveApplication(applications, filteringApplications);
            _MaxId = 0;
            return RedirectToAction("Index");

        }
        //отклонить заявку


        //Создать заявку
        public ActionResult CreateApplication(int id)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();

            id = Users.UserID;
            ViewBag.statusID = Users.mode.ToString();
            ViewBag.applications = 0;
            //Session["UserID"] = Users.UserID;

            MethodsRepository mr = new MethodsRepository();
            if (id != 0)
            {
                List<TypeCars> typeCars = db.TypeCars.ToList();
                List<Users> users = db.Users.ToList();

                //typeCars.Insert(0, new TypeCars { Name = "----------", Id = 0 });
                FilteringApplications aplc = new FilteringApplications
                {
                    application = new Applications(),
                    user = new Users(),
                    Users = users.ToList(),
                    TypeCars = new SelectList(typeCars, "Id", "Name")
                };
                aplc.user = mr.GetUserById(id);


                return View(aplc);
            }
            else
                return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateApplication(Applications applications, FilteringApplications filteringApplications, int id, string dateStart, string timeStart, string dateEnd, string timeEnd, string routeWherefrom, string routeWhere)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            //TempData["message"] = string.Format("Вы добавили продукт \"{0}\"!", product.Brand + " " + product.Model);

            ViewBag.statusID = Users.mode.ToString();
            ViewBag.applications = 0;

            if (DateTime.Parse(dateStart).Date + DateTime.Parse(timeStart).TimeOfDay >= DateTime.Parse(dateEnd).Date + DateTime.Parse(timeEnd).TimeOfDay)
            {
                ModelState.AddModelError("", "Время начала превышает время завершения!");
            }
            if (DateTime.Parse(timeStart).TimeOfDay <= DateTime.Now.TimeOfDay)
            {
                ModelState.AddModelError("", "Подкорректируйте время!");
            }


            //if (filteringApplications.application.TypeCarID == 1 && filteringApplications.application.QuantityPassengers == 0)
            //{
            //    ModelState.AddModelError("", "Укажите количество пассажиров!");
            //}
            //if (filteringApplications.application.TypeCarID == 2 && filteringApplications.application.CargoWeight == null)
            //{
            //    ModelState.AddModelError("", "Укажите вес груза!");
            //}

            else
            {
                filteringApplications.application.StartDate = DateTime.Parse(dateStart).Date + DateTime.Parse(timeStart).TimeOfDay;
                filteringApplications.application.EndDate = DateTime.Parse(dateEnd).Date + DateTime.Parse(timeEnd).TimeOfDay;
                filteringApplications.application.Route = routeWherefrom + " —> " + routeWhere;
                db.Applications.Add(filteringApplications.application);
                db.SaveChanges();
                //MethodsRepository app = new MethodsRepository();
                //app.SaveApplication(applications, filteringApplications);
                return RedirectToAction("Index");
            }

            MethodsRepository mr = new MethodsRepository();
            List<TypeCars> typeCars = db.TypeCars.ToList();
            List<Users> users = db.Users.ToList();
            FilteringApplications aplc = new FilteringApplications
            {
                application = new Applications(),
                user = new Users(),
                Users = users.ToList(),
                TypeCars = new SelectList(typeCars, "Id", "Name")
            };
            aplc.user = mr.GetUserById(id);
            _MaxId = 0;
            return View(aplc);

            //return View();
        }
        //Создать заявку



        //[HttpPost]
        //public ActionResult Index(int id = 0)
        //{
        //    //ASUZ_Transport_DBEntities db = new ASUZ_Transport_DBEntities();
        //    //List<Applications> applications = db.Applications.ToList();

        //    MethodsRepository methodsRepository = new MethodsRepository();
        //    //applications = methodsRepository.GetApplications();

        //    //if (id != 0)
        //    //{
        //    Applications model = new Applications();
        //    model = methodsRepository.GetApplicationById(id);
        //    Session["ApplicationId"] = "5252";

        //    return View(model);
        //    //}

        //    //FilteringApplications aplc = new FilteringApplications
        //    //{
        //    //    Applications = applications.ToList(),
        //    //};

        //    //return View(/*aplc);
        //}


        //[HttpPost]
        //public ActionResult Index(int id = 0)
        //{
        //    if (id != 0)
        //    {
        //        FilteringApplications filteringApplications = new FilteringApplications();

        //        filteringApplications.application = new Applications();
        //        MethodsRepository methodsRepository = new MethodsRepository();
        //        filteringApplications.application = methodsRepository.GetApplicationById(id);

        //        return View(filteringApplications.application);
        //    }
        //    else
        //        return RedirectToAction("Index");
        //}



        //List<string> drc = new List<string>();

        //foreach (var i in applications)
        //{
        //    drc.Add(i.UserID.ToString());
        //}

        //List<Users> users = db.Users.ToList();
        //List<string> urc = new List<string>();

        //foreach (var i in users)
        //{
        //    urc.Add(i.Id.ToString());
        //}







        //ViewBag.drc = drc;

        //var app = applications.FirstOrDefault();
        //var usr = db.Users.FirstOrDefault(x=>x.Id == app.UserID);
        //List<Users> usrDrc = db.Users.Where(x => x.DivisionID == usr.DivisionID && x.StatusID == 3).ToList();

        //foreach (var usrDrc in db.Users.Where(x => x.DivisionID == usr.DivisionID && x.StatusID == 3)) 
        //{
        //    urc.Add(usrDrc.SurName);
        //}
        //ViewBag.drc = urc;


        //Session["usrDrc"] = usrDrc1.SurName + " " + usrDrc1.Name + " " + usrDrc1.Partonymic;

        //foreach (var i in db.Users.Where(x => x.DivisionID == usr.DivisionID && x.StatusID == 3))
        //{
        //    urc.Add(i.SurName + i.Name + i.Partonymic);
        //}
        //ViewBag.urc = urc;
    }
}