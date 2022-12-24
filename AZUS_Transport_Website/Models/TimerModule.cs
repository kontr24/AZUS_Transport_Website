using AZUS_Transport_Website.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.Web;

namespace AZUS_Transport_Website.Models
{
    public class TimerModule : IHttpModule
    {
        static Timer timer;
        long interval = 45000; //45 секунд
        static object synclock = new object();
        static bool sent = false;

        public void Init(HttpApplication app)
        {
            timer = new Timer(new TimerCallback(SendEmail), null, 0, interval);// запускается таймер, который каждые 30000 миллисекунд (30 секунд) вызывает метод SendEmail()
            timer = new Timer(new TimerCallback(tReport), null, 0, interval);
        }


        //если время на согласование вышло, автоматически "не исполнено"
        private void SendEmail(object obj)
        {
            lock (synclock)//определяет критическую секцию, доступ к которой одновременно возможен только для одного потока
            {
                //DateTime dd = DateTime.Now;
                //if (dd.Hour == 17 && dd.Minute == 06 && sent == false)
                //{
                if (sent == false) //Для управления отправкой мы проверяем значение переменной sent
                {

                    using (var db = new ASUZ_Transport_DBEntitie())
                    {
                        var ap = db.Applications.AsEnumerable().Where(x => (x.DispatcherATA_StatusDoneID == 3)
                        && (DateTime.Now - x.DateCreation > new TimeSpan(1, 0, 0, 0))
                        && (x.DirectorStatusDoneID == 3 || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3)).FirstOrDefault();

                        if (ap != null)
                        {

                            foreach (var app in db.Applications.AsEnumerable().Where(x => (x.DispatcherATA_StatusDoneID != 4 && x.DispatcherATA_StatusDoneID != 5)
                            && (DateTime.Now - x.DateCreation > new TimeSpan(1, 0, 0, 0))
                            && (x.DirectorStatusDoneID == 3 || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3)))
                            {
                                app.DispatcherATA_StatusDoneID = 5;
                                var crs = db.Cars.Where(x => x.Id == app.CarID).FirstOrDefault();
                                crs.StatusCarID = 1;

                                var usr = db.Users.FirstOrDefault(x => x.StatusID == 7);
                                var us = db.Users.FirstOrDefault(x => x.Id == app.UserID);
                                //Почта
                                SmtpClient Smtp = new SmtpClient("smtp.mail.ru", 2525);
                                Smtp.Credentials = new NetworkCredential(usr.Email, "DC7zZ5CvANF9GZhzK65V");
                                MailMessage Message = new MailMessage();
                                Smtp.EnableSsl = true;
                                Message.From = new MailAddress(usr.Email);
                                Message.To.Add(new MailAddress(us.Email));
                                Message.Subject = "Статус заявки № " + app.Id;
                                Message.IsBodyHtml = true;//html отображение
                                Message.Body = "Ваша заявка не исполнена, потому что время на её согласование вышло!" + "<br/><br/>Перейти в " + "<a href=\"https://localhost:44324/Account/Login\">АСУЗ 'Транспорт'</a>" + "<style> .colorDiv {color: #d0d4ce}</style>" + "<br/><br/><hr><div class='colorDiv'>Данное письмо было сформировано автоматически подсистемой уведомления АСУЗ 'Транспорт'. Ответ не требуется</div>";

                                try
                                {
                                    Smtp.Send(Message);

                                }
                                catch (Exception/* ex*/)
                                {
                                    return;
                                }

                            }
                            db.SaveChanges();
                        }
                    }


                    sent = true;
                }
                else
                {
                    sent = false;
                }
            }
        }
        //если время на согласование вышло, автоматически "не исполнено"

        //отправка отчётов (еженедельный, ежедневный)
        public void tReport(object obj)
        {
            int hour = 12;
            int minute = 00;

            if ((DateTime.Now.DayOfWeek == DayOfWeek.Friday) && (hour == DateTime.Now.Hour) && (minute == DateTime.Now.Minute))
            {
                using (var db = new ASUZ_Transport_DBEntitie())
                {
                    var usr = db.Users.FirstOrDefault(x => x.StatusID == 7);
                    var us = db.Users.FirstOrDefault(x => x.StatusID == 5);
                    //Почта
                    SmtpClient Smtp = new SmtpClient("smtp.mail.ru", 2525);
                    Smtp.Credentials = new NetworkCredential(us.Email, "DC7zZ5CvANF9GZhzK65V");
                    MailMessage Message = new MailMessage();
                    Smtp.EnableSsl = true;
                    Message.From = new MailAddress(us.Email);
                    Message.To.Add(new MailAddress(usr.Email));
                    Message.Subject = "Отчёт по заявкам за период: " + String.Format("{0:d}", DateTime.Now.AddDays(-7)) + " - " + String.Format("{0:d}", DateTime.Now);
                    Message.IsBodyHtml = true;//html отображение

                    //посчитать количество определенных одинаковых данных
                    var firstColumnValuesDirector = db.Applications.AsEnumerable().Where(x => (x.DateCreation > DateTime.Now.AddDays(-7) && x.DirectorStatusDoneID != 3) || x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.DirectorStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                    Dictionary<int, int> resultDirector = firstColumnValuesDirector.ToDictionary(arg => arg.Key, arg => arg.Count);

                    int agreedDirector;
                    resultDirector.TryGetValue(1, out agreedDirector);
                    int considerationDirector;
                    resultDirector.TryGetValue(3, out considerationDirector);
                    int rejectedDirector;
                    resultDirector.TryGetValue(2, out rejectedDirector);


                    var firstColumnValuesEconomist = db.Applications.AsEnumerable().Where(x => (x.DateCreation > DateTime.Now.AddDays(-7) && x.EconomistStatusDoneID != 3) || x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.EconomistStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                    Dictionary<int, int> resultEconomist = firstColumnValuesEconomist.ToDictionary(arg => arg.Key, arg => arg.Count);

                    int agreedEconomist;
                    resultEconomist.TryGetValue(1, out agreedEconomist);
                    int considerationEconomist;
                    resultEconomist.TryGetValue(3, out considerationEconomist);
                    int rejectedEconomist;
                    resultEconomist.TryGetValue(2, out rejectedEconomist);

                    var firstColumnValuesDepartment = db.Applications.AsEnumerable().Where(x => (x.DateCreation > DateTime.Now.AddDays(-7) && x.DepartmentStatusDoneID != 3 && (x.IntercityСity != true || x.Days != true)) || (x.DateCreation.AddDays(1) > DateTime.Now && (x.IntercityСity != true || x.Days != true))).GroupBy(x => x.DepartmentStatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                    Dictionary<int, int> resultDepartment = firstColumnValuesDepartment.ToDictionary(arg => arg.Key, arg => arg.Count);

                    int agreedDepartment;
                    resultDepartment.TryGetValue(1, out agreedDepartment);
                    int considerationDepartment;
                    resultDepartment.TryGetValue(3, out considerationDepartment);
                    int rejectedDepartment;
                    resultDepartment.TryGetValue(2, out rejectedDepartment);

                    var firstColumnValuesDispatcherNIIAR_StatusDone = db.Applications.AsEnumerable().Where(x => (x.DateCreation > DateTime.Now.AddDays(-7) && x.DispatcherNIIAR_StatusDoneID != 3) || x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.DispatcherNIIAR_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                    Dictionary<int, int> resultDispatcherNIIAR_StatusDone = firstColumnValuesDispatcherNIIAR_StatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                    int agreedDispatcherNIIAR_StatusDone;
                    resultDispatcherNIIAR_StatusDone.TryGetValue(1, out agreedDispatcherNIIAR_StatusDone);
                    int considerationDispatcherNIIAR_StatusDone;
                    resultDispatcherNIIAR_StatusDone.TryGetValue(3, out considerationDispatcherNIIAR_StatusDone);
                    int rejectedDispatcherNIIAR_StatusDone;
                    resultDispatcherNIIAR_StatusDone.TryGetValue(2, out rejectedDispatcherNIIAR_StatusDone);

                    var firstColumnValuesDispatcherATA_StatusDone = db.Applications.AsEnumerable().Where(x => (x.DateCreation > DateTime.Now.AddDays(-7) && x.DispatcherNIIAR_StatusDoneID != 3) || (x.DateCreation.AddDays(1) > DateTime.Now)).GroupBy(x => x.DispatcherATA_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                    Dictionary<int, int> resultDispatcherATA_StatusDone = firstColumnValuesDispatcherATA_StatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                    int ExecutedDispatcherATA_StatusDone;
                    resultDispatcherATA_StatusDone.TryGetValue(4, out ExecutedDispatcherATA_StatusDone);
                    int NotExecutedDispatcherATA_StatusDone;
                    resultDispatcherATA_StatusDone.TryGetValue(5, out NotExecutedDispatcherATA_StatusDone);
                    //посчитать количество определенных одинаковых данных
                    var firstColumnValuesSumm = db.Applications.AsEnumerable().Where(x => x.DateCreation > DateTime.Now.AddDays(-7)).Count();
                    var firstColumnValuesTimeUpSumm = db.Applications.AsEnumerable().Where(x => x.DateCreation > DateTime.Now.AddDays(-7) && (x.DateCreation.AddDays(1) < DateTime.Now && (x.DirectorStatusDoneID == 3 || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3))).Count();

                    Message.Body = "Количество поступивших заявок: " + firstColumnValuesSumm + "<br/><br/>" + "Из них: " + "<table class='table'><thead><tr><th class='text-center'>Текущий статус</th><th class='text-right_'>Количество</th></tr><tr><th class='text-left'>На рассмотрении у руководителей</th><th class='text-right'>" + considerationDirector + "</th></tr><tr><th class='text-left'>На рассмотрении у экономистов</th> <th class='text-right'>" + considerationEconomist + "</th></tr><tr><th class='text-left'>На рассмотрении у ДИД</th><th class='text-right'>" + considerationDepartment + "</th></tr><tr> <th class='text-left'>На рассмотрении у диспетчера НИИАР</th><th class='text-right'>" + considerationDispatcherNIIAR_StatusDone + "</th></tr><tr><th class='text-left'>Отклонены руководителями</th><th class='text-right'>" + rejectedDirector + "</th></tr><tr><th class='text-left'>Отклонены экономистами</th><th class='text-right'>" + rejectedEconomist + "</th></tr><tr><th class='text-left'>Отклонены ДИД</th><th class='text-right'>" + rejectedDepartment + "</th></tr><tr><th class='text-left'>Отклонены диспетчером НИИАР</th><th class='text-right'>" + rejectedDispatcherNIIAR_StatusDone + "</th></tr><tr><th class='text-left'>Согласовано руководителями</th><th class='text-right'>" + agreedDirector + "</th></tr><tr><th class='text-left'>Согласовано экономистами</th><th class='text-right'>" + agreedEconomist + "</th></tr><tr><th class='text-left'>Согласовано ДИД</th><th class='text-right'>" + agreedDepartment + "</th></tr><tr><th class='text-left'>Согласовано у диспетчера НИИАР</th><th class='text-right'>" + agreedDispatcherNIIAR_StatusDone + "</th></tr><tr><th class='text-left'>Исполнено</th><th class='text-right'>" + ExecutedDispatcherATA_StatusDone + "</th></tr><tr><th class='text-left'>Не исполнено</th><th class='text-right'>" + NotExecutedDispatcherATA_StatusDone + "</th></tr><tr><th class='text-left'>Время согласования истекло</th><th class='text-right'>" + firstColumnValuesTimeUpSumm + "</th></tr></thead></table>" +
                            "<style>  .table { width: auto; margin-bottom: 20px; border: 5px solid #edd14b; border-collapse: collapse; margin: 0px 0px 0px 0px }.table th {  width: 280px; padding: 5px; } .table td {  border: 3px solid #edd14b; padding: 5px; } .text-center {text-align: center; background: #edd14b;}  .text-right_ {text-align: right; background: #edd14b;} .text-left {text-align: left;border: 1px solid #edd14b; font-weight: 100;} .text-right {text-align: right;border: 1px solid #edd14b;font-weight: 100;} div{color: #d0d4ce}</style>" + "<br/>" + "Дата и время формирования отчёта: " + String.Format("{0:d} - {0:t}", DateTime.Now) + "<br/><br/>" + "Перейти в " + "<a href=\"https://localhost:44324/Account/Login\">АСУЗ 'Транспорт'</a>" + "<hr><div>Данное письмо было сформировано автоматически подсистемой уведомления АСУЗ 'Транспорт'. Ответ не требуется</div>";

                    //1 Вариант
                    //Message.Body = "Количество поступивщих заявок: " + firstColumnValuesSumm + "<br/><br/>" + "Из них: " + "<table class='table'><thead><tr><th class='text-center'>Текущий статус</th><th class='text-right'>Количество</th></tr></thead> <tbody><tr><td class='text-left'>На рассмотрении у руководителя<br/><br/>На рассмотрении у экономиста<br/><br/>На рассмотрении у ДИД<br/><br/>На рассмотрении у диспетчера НИИАР<br/><br/>Отклонена руководителем<br/><br/>Отклонена экономистом<br/><br/>Отклонена ДИД<br/><br/>Отклонена диспетчером НИИАР<br/><br/>Согласовано руководителем<br/><br/>Согласовано экономистом<br/><br/>Согласовано ДИД<br/><br/>Согласовано Диспетчером НИИАР<br/><br/>Исполнено<br/><br/>Не исполнено</td><td class='text-right'>"
                    //   + considerationDirector + "<br/><br/>" + considerationEconomist + "<br/><br/>" + considerationDepartment + "<br/><br/>" + considerationDispatcherNIIAR_StatusDone + "<br/><br/>" + rejectedDirector + "<br/><br/>" + rejectedEconomist + "<br/><br/>" + rejectedDepartment + "<br/><br/>" + rejectedDispatcherNIIAR_StatusDone + "<br/><br/>" + agreedDirector + "<br/><br/>" + agreedEconomist + "<br/><br/>" + agreedDepartment + "<br/><br/>" + agreedDispatcherNIIAR_StatusDone + "<br/><br/>" + ExecutedDispatcherATA_StatusDone + "<br/><br/>" + NotExecutedDispatcherATA_StatusDone + "</td></tr></tbody></table>" +
                    //   "<style>  .table { width: auto; margin-bottom: 20px; border: 5px solid #edd14b; border-collapse: collapse; margin: 0px 0px 0px 0px }.table th {  width: 280px; font-weight: bold; padding: 5px; background: #edd14b; } .table td {  border: 3px solid #edd14b; padding: 5px; } .text-center {text-align: center;} .text-left {text-align: left;} .text-right {text-align: right;}</style>" + "<br/><br/>" + "Данное письмо было сформировано автоматически подсистемой уведомления АСУЗ 'Транспорт'. Ответ не требуется";
                    //1 Вариант
                    try
                    {
                        Smtp.Send(Message);
                    }
                    catch (Exception /*ex*/)
                    {
                        //MessageBox.Show(ex.Message);
                        return;
                    }


                }
            }


            if ((hour == DateTime.Now.Hour) && (minute == DateTime.Now.Minute))
            {

                using (var db = new ASUZ_Transport_DBEntitie())
                {
                    var usr = db.Users.FirstOrDefault(x => x.StatusID == 7);
                    var us = db.Users.FirstOrDefault(x => x.StatusID == 5);
                    //Почта
                    SmtpClient Smtp = new SmtpClient("smtp.mail.ru", 2525);
                    Smtp.Credentials = new NetworkCredential(us.Email, "DC7zZ5CvANF9GZhzK65V");
                    MailMessage Message = new MailMessage();
                    Smtp.EnableSsl = true;
                    Message.From = new MailAddress(us.Email);
                    Message.To.Add(new MailAddress(usr.Email));
                    Message.Subject = "Отчёт по заявкам за " + DateTime.Now;
                    Message.IsBodyHtml = true;//html отображение

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

                    var firstColumnValuesDispatcherNIIAR_StatusDone = db.Applications.AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.DispatcherNIIAR_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                    Dictionary<int, int> resultDispatcherNIIAR_StatusDone = firstColumnValuesDispatcherNIIAR_StatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                    int agreedDispatcherNIIAR_StatusDone;
                    resultDispatcherNIIAR_StatusDone.TryGetValue(1, out agreedDispatcherNIIAR_StatusDone);
                    int considerationDispatcherNIIAR_StatusDone;
                    resultDispatcherNIIAR_StatusDone.TryGetValue(3, out considerationDispatcherNIIAR_StatusDone);
                    int rejectedDispatcherNIIAR_StatusDone;
                    resultDispatcherNIIAR_StatusDone.TryGetValue(2, out rejectedDispatcherNIIAR_StatusDone);

                    var firstColumnValuesDispatcherATA_StatusDone = db.Applications.AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).GroupBy(x => x.DispatcherATA_StatusDoneID).Where(x => x.Key != 0).Select(x => new { x.Key, Count = x.Count() });
                    Dictionary<int, int> resultDispatcherATA_StatusDone = firstColumnValuesDispatcherATA_StatusDone.ToDictionary(arg => arg.Key, arg => arg.Count);

                    int ExecutedDispatcherATA_StatusDone;
                    resultDispatcherATA_StatusDone.TryGetValue(4, out ExecutedDispatcherATA_StatusDone);
                    int NotExecutedDispatcherATA_StatusDone;
                    resultDispatcherATA_StatusDone.TryGetValue(5, out NotExecutedDispatcherATA_StatusDone);
                    //посчитать количество определенных одинаковых данных
                    var firstColumnValuesSumm = db.Applications.AsEnumerable().Where(x => x.DateCreation.AddDays(1) > DateTime.Now).Count();

                    Message.Body = "Количество поступивщих заявок: " + firstColumnValuesSumm + "<br/><br/>" + "Из них: " + "<table class='table'><thead><tr><th class='text-center'>Текущий статус</th><th class='text-right_'>Количество</th></tr><tr><th class='text-left'>На рассмотрении у руководителей</th><th class='text-right'>" + considerationDirector + "</th></tr><tr><th class='text-left'>На рассмотрении у экономистов</th> <th class='text-right'>" + considerationEconomist + "</th></tr><tr><th class='text-left'>На рассмотрении у ДИД</th><th class='text-right'>" + considerationDepartment + "</th></tr><tr> <th class='text-left'>На рассмотрении у диспетчера НИИАР</th><th class='text-right'>" + considerationDispatcherNIIAR_StatusDone + "</th></tr><tr><th class='text-left'>Отклонены руководителями</th><th class='text-right'>" + rejectedDirector + "</th></tr><tr><th class='text-left'>Отклонены экономистами</th><th class='text-right'>" + rejectedEconomist + "</th></tr><tr><th class='text-left'>Отклонены ДИД</th><th class='text-right'>" + rejectedDepartment + "</th></tr><tr><th class='text-left'>Отклонены диспетчером НИИАР</th><th class='text-right'>" + rejectedDispatcherNIIAR_StatusDone + "</th></tr><tr><th class='text-left'>Согласовано руководителями</th><th class='text-right'>" + agreedDirector + "</th></tr><tr><th class='text-left'>Согласовано экономистами</th><th class='text-right'>" + agreedEconomist + "</th></tr><tr><th class='text-left'>Согласовано ДИД</th><th class='text-right'>" + agreedDepartment + "</th></tr><tr><th class='text-left'>Согласовано у диспетчера НИИАР</th><th class='text-right'>" + agreedDispatcherNIIAR_StatusDone + "</th></tr><tr><th class='text-left'>Исполнено</th><th class='text-right'>" + ExecutedDispatcherATA_StatusDone + "</th></tr><tr><th class='text-left'>Не исполнено</th><th class='text-right'>" + NotExecutedDispatcherATA_StatusDone + "</th></tr></thead></table>" +
                        "<style>  .table { width: auto; margin-bottom: 20px; border: 5px solid #edd14b; border-collapse: collapse; margin: 0px 0px 0px 0px }.table th {  width: 280px; padding: 5px; } .table td {  border: 3px solid #edd14b; padding: 5px; } .text-center {text-align: center; background: #edd14b;}  .text-right_ {text-align: right; background: #edd14b;} .text-left {text-align: left;border: 1px solid #edd14b; font-weight: 100;} .text-right {text-align: right;border: 1px solid #edd14b;font-weight: 100;} div{color: #d0d4ce}</style>" + "<br/>" + "<br/>" + "Дата и время формирования отчёта: " + String.Format("{0:d} - {0:t}", DateTime.Now) + "<br/>" + "<br/>Перейти в " + "<a href=\"https://localhost:44324/Account/Login\">АСУЗ 'Транспорт'</a>" + "<hr><div>Данное письмо было сформировано автоматически подсистемой уведомления АСУЗ 'Транспорт'. Ответ не требуется</div>";


                    try
                    {
                        Smtp.Send(Message);

                    }
                    catch (Exception /*ex*/)
                    {
                        //MessageBox.Show(ex.Message);
                        return;
                    }
                }

            }
        }
        //отправка отчётов (еженедельный, ежедневный)

        public void Dispose()
        { }
    }


}