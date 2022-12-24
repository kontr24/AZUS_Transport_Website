using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AZUS_Transport_Website.Models
{
    public class MethodsRepository
    {

        public List<Applications> GetApplications(bool archiveTrue, bool archiveFalse)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            List<Applications> applications = db.Applications.ToList();

            //var usrDrc = db.Users.FirstOrDefault(x => x.DivisionID ==   && x.StatusID == 3);
            if (Users.mode == (int)Users.Status.Admin)
            {

                if (archiveTrue == true)
                {
                    applications = db.Applications.AsEnumerable()/*.Include(x => x.Users).Include(x => x.TypeCars).Include(x => x.Cars).Include(x => x.StatusesDone).*/.Where(x => /*x.StartDate > DateTime.Now &&*/ DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) &&
                             ((x.DirectorStatusDoneID == 3 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                             (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                             (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                             (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 3) ||
                             (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 1 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3) ||
                             (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1 && x.DepartmentStatusDoneID == 3 && x.DispatcherNIIAR_StatusDoneID == 1 && x.DispatcherATA_StatusDoneID == 3))).ToList();

                }
                if (archiveFalse == true)
                {
                    applications = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation > new TimeSpan(1, 0, 0, 0) || ((x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && (x.DispatcherNIIAR_StatusDoneID == 1 ||
                          x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2))).ToList();
                }

            }

                if (Users.mode == (int)Users.Status.Client)
            {
                if (archiveTrue == true)
                {
                    applications = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) && ((x.UserID == Users.UserID && x.DirectorStatusDoneID == 3) ||
                (x.UserID == Users.UserID && x.DirectorStatusDoneID != 2) && (x.UserID == Users.UserID && x.EconomistStatusDoneID != 2) &&
                (x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID != 2) && (x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID != 1)
                 && (x.UserID == Users.UserID && x.DepartmentStatusDoneID != 2))).ToList();
                }
                if (archiveFalse == true)
                {
                    applications = db.Applications.AsEnumerable().Where(x => (((x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID == 1) ||
                (x.UserID == Users.UserID && x.DirectorStatusDoneID == 2) || (x.UserID == Users.UserID && x.EconomistStatusDoneID == 2) || (x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID == 2) || (x.UserID == Users.UserID && x.DepartmentStatusDoneID == 2))) ||
                (DateTime.Now - x.DateCreation > new TimeSpan(1, 0, 0, 0) && ((x.UserID == Users.UserID && x.DirectorStatusDoneID == 3) ||
                 (x.UserID == Users.UserID && x.EconomistStatusDoneID == 3) ||
                (x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID == 3) || (x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID == 3)
                 && (x.UserID == Users.UserID && x.DepartmentStatusDoneID == 3) || (x.UserID == Users.UserID && x.DirectorStatusDoneID == 1) ||
                 (x.UserID == Users.UserID && x.EconomistStatusDoneID == 1) || (x.UserID == Users.UserID && x.DepartmentStatusDoneID == 1)))).ToList();

                }
            }


            if (Users.mode == (int)Users.Status.Director)
            {
                var usDrc = db.Users.FirstOrDefault(x => x.Id == Users.UserID);
                var us = db.Users.FirstOrDefault(x => x.DivisionID == usDrc.DivisionID);

                applications = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) && (x.DirectorStatusDoneID == 3) && x.UserID == us.Id).ToList();
            }

            if (Users.mode == (int)Users.Status.Economist)
            {
                var usEcn = db.Users.FirstOrDefault(x => x.Id == Users.UserID);
                var usr = db.Users.FirstOrDefault(x => x.DivisionID == usEcn.DivisionID);

                applications = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) && (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3) && x.UserID == usr.Id).ToList();
            }
            if (Users.mode == (int)Users.Status.Department)
            {
                applications = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) && (x.DepartmentStatusDoneID == 3 && x.EconomistStatusDoneID == 1 && (x.IntercityСity == false || x.Days == false))).ToList();

            }

            if (Users.mode == (int)Users.Status.DispatcherNIIAR)
            {
                applications = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) && ((x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1
                && x.DepartmentStatusDoneID == 1
        && x.DispatcherNIIAR_StatusDoneID == 3 && ((x.Days == true && x.IntercityСity == false) ||
         (x.Days == false && x.IntercityСity == true) || (x.Days == false && x.IntercityСity == false)
       || (x.Days == true && x.IntercityСity == true))) || (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1
                && x.DepartmentStatusDoneID == 3
        && x.DispatcherNIIAR_StatusDoneID == 3 && (x.Days == true && x.IntercityСity == true)))).ToList();

            }

            if (Users.mode == (int)Users.Status.DispatcherATA)
            {
                if (archiveTrue == true)
                {
                    applications = db.Applications.AsEnumerable().Where(x => (
               (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
               (x.DispatcherNIIAR_StatusDoneID != 3)
                && (x.DepartmentStatusDoneID != 3) && x.DispatcherATA_StatusDoneID == 3) || (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
               (x.DispatcherNIIAR_StatusDoneID != 3)
                && (x.DepartmentStatusDoneID == 3) && x.DispatcherATA_StatusDoneID == 3).ToList();

                }
                if (archiveFalse == true)
                {
                    applications = db.Applications.AsEnumerable().Where(x => (x.DispatcherATA_StatusDoneID == 4 || x.DispatcherATA_StatusDoneID == 5) && ((x.DispatcherNIIAR_StatusDoneID == 1 ||
                        x.DirectorStatusDoneID == 2 || x.EconomistStatusDoneID == 2 || x.DispatcherNIIAR_StatusDoneID == 2 || x.DepartmentStatusDoneID == 2) || (DateTime.Now - x.DateCreation > new TimeSpan(1, 0, 0, 0) && (x.DirectorStatusDoneID == 3) || x.EconomistStatusDoneID == 3 || x.DepartmentStatusDoneID == 3 || x.DispatcherNIIAR_StatusDoneID == 3))).ToList();
                }

            }
            return applications;
        }

        public void SaveApplication(Applications applications,FilteringApplications filteringApplications/*, int drcAccept, int drcReject*/)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            //if (filteringApplications.application.Id == 0)
            //{
            //    db.Applications.Add(filteringApplications.application);

            //}
            Applications dbEntry = db.Applications.Find(applications.Id);
            if (dbEntry != null)
            {
                if (Users.mode == (int)Users.Status.Director)
                {
                    dbEntry.DirectorStatusDoneID = applications.DirectorStatusDoneID;
                }
                if (Users.mode == (int)Users.Status.Economist)
                {
                    dbEntry.EconomistStatusDoneID = applications.EconomistStatusDoneID;
                    dbEntry.CPC = applications.CPC;
                }
                if (Users.mode == (int)Users.Status.Department)
                {
                    dbEntry.DepartmentStatusDoneID = applications.DepartmentStatusDoneID;
                }
                if (Users.mode == (int)Users.Status.DispatcherNIIAR)
                {
                    dbEntry.DispatcherNIIAR_StatusDoneID = applications.DispatcherNIIAR_StatusDoneID;
                }
                if (Users.mode == (int)Users.Status.DispatcherATA)
                {
                    dbEntry.DispatcherATA_StatusDoneID = applications.DispatcherATA_StatusDoneID;
                }
                db.SaveChanges();

            }
        }

        public Users GetUserById(int id)
        {
            ASUZ_Transport_DBEntitie db = new ASUZ_Transport_DBEntitie();
            FilteringApplications model = new FilteringApplications();
            model.user = null;

            model.user = db.Users.First(row => row.Id == id);
            return model.user;
        }


        //максимальный id
        public int CheckMaxId()
        {
            int tek_max_id = 0;
            using (var db = new ASUZ_Transport_DBEntitie())
            {
                if (Users.mode == (int)Users.Status.Director)
                {

                    var usDrc = db.Users.FirstOrDefault(x => x.Id == Users.UserID);
                    var us = db.Users.FirstOrDefault(x => x.DivisionID == usDrc.DivisionID);
                    var idMax = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) && (x.DirectorStatusDoneID == 3) && x.UserID == us.Id).Max(x => x.Id);

                    tek_max_id = idMax;
                }
                if (Users.mode == (int)Users.Status.Economist)
                {
                    var usEcn = db.Users.FirstOrDefault(x => x.Id == Users.UserID);
                    var usr = db.Users.FirstOrDefault(x => x.DivisionID == usEcn.DivisionID);

                    var idMax = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) && (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 3) && x.UserID == usr.Id).Max(x => x.Id);
                    tek_max_id = idMax;
                }
                if (Users.mode == (int)Users.Status.Department)
                {
                    var idMax = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) && (x.DepartmentStatusDoneID == 3 && x.EconomistStatusDoneID == 1 && (x.IntercityСity == false || x.Days == false))).Max(x => x.Id);
                    tek_max_id = idMax;
                }
                if (Users.mode == (int)Users.Status.DispatcherNIIAR)
                {
                    var idMax = db.Applications.AsEnumerable().Where(x => DateTime.Now - x.DateCreation < new TimeSpan(1, 0, 0, 0) && ((x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1
                    && x.DepartmentStatusDoneID == 1
            && x.DispatcherNIIAR_StatusDoneID == 3 && ((x.Days == true && x.IntercityСity == false) ||
             (x.Days == false && x.IntercityСity == true) || (x.Days == false && x.IntercityСity == false)
           || (x.Days == true && x.IntercityСity == true))) || (x.DirectorStatusDoneID == 1 && x.EconomistStatusDoneID == 1
                    && x.DepartmentStatusDoneID == 3
            && x.DispatcherNIIAR_StatusDoneID == 3 && (x.Days == true && x.IntercityСity == true)))).Max(x => x.Id);
                    tek_max_id = idMax;
                }
                if (Users.mode == (int)Users.Status.DispatcherATA)
                {
                    var idMax = db.Applications.AsEnumerable().Where(x => (
               (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
               (x.DispatcherNIIAR_StatusDoneID != 3)
                && (x.DepartmentStatusDoneID != 3) && x.DispatcherATA_StatusDoneID == 3) || (x.DirectorStatusDoneID != 3) && (x.EconomistStatusDoneID != 3) &&
               (x.DispatcherNIIAR_StatusDoneID != 3)
                && (x.DepartmentStatusDoneID == 3) && x.DispatcherATA_StatusDoneID == 3).Max(x => x.Id);
                    tek_max_id = idMax;
                }
                if (Users.mode == (int)Users.Status.Client)
                {
                    var idMax = db.Applications.AsEnumerable().Where(x => (((x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID == 1) ||
                      (x.UserID == Users.UserID && x.DirectorStatusDoneID == 2) || (x.UserID == Users.UserID && x.EconomistStatusDoneID == 2) || (x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID == 2) || (x.UserID == Users.UserID && x.DepartmentStatusDoneID == 2))) ||
                      (DateTime.Now - x.DateCreation > new TimeSpan(1, 0, 0, 0) && ((x.UserID == Users.UserID && x.DirectorStatusDoneID == 3) ||
                       (x.UserID == Users.UserID && x.EconomistStatusDoneID == 3) ||
                      (x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID == 3) || (x.UserID == Users.UserID && x.DispatcherNIIAR_StatusDoneID == 3)
                       && (x.UserID == Users.UserID && x.DepartmentStatusDoneID == 3) || (x.UserID == Users.UserID && x.DirectorStatusDoneID == 1) ||
                       (x.UserID == Users.UserID && x.EconomistStatusDoneID == 1) || (x.UserID == Users.UserID && x.DepartmentStatusDoneID == 1)))).Max(x => x.Id);
                    tek_max_id = idMax;
                }
            }
            return tek_max_id;
        }


        //public Applications GetApplicationId(int id)
        //{
        //    ASUZ_Transport_DBEntities db = new ASUZ_Transport_DBEntities();

        //    Applications product = null;

        //    product = db.Applications.First(row => row.Id == id);

        //    if (product != null /* && applications.CategoryId != 0*/)
        //    {
        //        dbEntry.DirectorStatusDoneID = applications.DirectorStatusDoneID;


        //    }

        //    //}

        //    db.SaveChanges();
        //    return product;
        //}



        //public Applications GetApplicationById(int id)
        //{
        //    ASUZ_Transport_DBEntities db = new ASUZ_Transport_DBEntities();
        //    FilteringApplications filteringApplications = new FilteringApplications();
        //    filteringApplications.application = null;

        //    filteringApplications.application = db.Applications.First(row => row.Id == id);

        //    return filteringApplications.application;
        //}

    }
}