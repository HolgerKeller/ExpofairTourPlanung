﻿using ExpofairTourPlanung.Data;
using ExpofairTourPlanung.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ExpofairTourPlanung.Controllers
{
    public class EventController : Controller
    {
        private readonly ILogger<EventController> _logger;

        private readonly EasyjobDbContext _context;

        public EventController(EasyjobDbContext context, ILogger<EventController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(string dateFrom)
        {
            string dateTo = null;

            if (dateFrom == null)
            {

                dateFrom = this.HttpContext.Session.GetString("dateFrom");

                if (dateFrom == null)
                {
                    dateFrom = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }

            dateTo = dateFrom;

            ViewData["dateFrom"] = dateFrom;
            ViewData["dateTo"] = dateTo;

            this.HttpContext.Session.SetString("dateFrom", dateFrom);

            var dateFromParam = new SqlParameter()
            {
                ParameterName = "@DateStart",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 10,
                Value = dateFrom
            };

            var dateToParam = new SqlParameter()
            {
                ParameterName = "@DateEnd",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 10,
                Value = dateTo
            };

            var CopyJobs = _context.Database.ExecuteSqlRaw("exec expofair.CustCopyJobsByDate @DateStart, @DateEnd", dateFromParam, dateToParam);




            var dateFromParam1 = new SqlParameter()
            {
                ParameterName = "@Date",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 10,
                Value = dateFrom
            };


            var allEvents = _context.ExpoEvents.FromSqlRaw("exec expofair.GetExpoEvents @Date", dateFromParam1).ToList();



            return View("Index2", allEvents);
        }

        public IActionResult CreateTour(string date, string eventList)
        {
            ViewData["dateFrom"] = date;

            var dateFromParam = new SqlParameter()
            {
                ParameterName = "@EventDate",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 10,
                Value = date
            };

            var stringEvent = new SqlParameter()
            {
                ParameterName = "@EventString",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 4000,
                Value = eventList
            };

            var createTour = _context.Database.ExecuteSqlRaw("exec expofair.CreateTourFromEvents @EventDate, @EventString", dateFromParam, stringEvent);


            return RedirectToAction("Index", new { dateFrom = date });
        }


        //############################################ New


        public PartialViewResult GetEvents(string dateFrom, string dateTo)
        {

            //string dateTo = null;

            if (dateFrom == null)
            {
                dateFrom = this.HttpContext.Session.GetString("dateFrom");

                if (dateFrom == null)
                {
                    dateFrom = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }

            if (dateTo == null)
            {
                dateTo = this.HttpContext.Session.GetString("dateTo");

                if (dateTo == null)
                {
                    dateTo = dateFrom;
                }
            }


            ViewData["dateFrom"] = dateFrom;
            this.HttpContext.Session.SetString("dateFrom", dateFrom);

            ViewData["dateTo"] = dateTo;
            this.HttpContext.Session.SetString("dateTo", dateTo);

            _logger.LogInformation("dateFrom:" + dateFrom + " dateTo:" + dateTo);

            var dateFromParam = new SqlParameter()
            {
                ParameterName = "@DateStart",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 10,
                Value = dateFrom
            };

            var dateToParam = new SqlParameter()
            {
                ParameterName = "@DateEnd",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 10,
                Value = dateTo
            };

            var CopyJobs = _context.Database.ExecuteSqlRaw("exec expofair.CustCopyJobsByDate @DateStart, @DateEnd", dateFromParam, dateToParam);

            DateTime dateFromDT = DateTime.Parse(dateFrom);
            DateTime dateToDT = DateTime.Parse(dateTo);

            var allEvents = _context.Job2Tours.Where(x => x.JobDate >= dateFromDT && x.JobDate <= dateToDT && x.JobType != ".Bessemerstrasse" && x.JobType != null).Select(x => x.JobType).Distinct().ToList();

            _logger.LogInformation("Anzahl Events:" + allEvents.Count.ToString());

            return PartialView("_GetEvents", allEvents);
        }

        public PartialViewResult GetAddresses(string ev)
        {
            var eventParam = new SqlParameter()
            {
                ParameterName = "@Event",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 80,
                Value = ev
            };

            var allAddresses = _context.ExpoEvents.FromSqlRaw("exec expofair.GetEventAdd @Event", eventParam).ToList();

            return PartialView("_GetAddresses", allAddresses);

        }

        public PartialViewResult GetJobs(string addr, DateTime datum, string del)
        {
            var eventParam = new SqlParameter()
            {
                ParameterName = "@Event",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 10,
                Value = addr
            };

            var allAddresses = _context.ExpoEvents.FromSqlRaw("exec expofair.GetEventAdd @Event", eventParam).ToList();

            return PartialView("_GetJobs", allAddresses);

        }

    }
}

