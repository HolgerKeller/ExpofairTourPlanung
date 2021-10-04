﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iText.Layout;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Layout.Renderer;
using iText.Layout.Layout;
using iText.Kernel.Pdf.Canvas;
using ExpofairTourPlanung.Data;
using Microsoft.Extensions.Logging;
using ExpofairTourPlanung.Models;
using iText.Layout.Borders;
using Microsoft.EntityFrameworkCore;

namespace ExpofairTourPlanung.Controllers
{
    public class TourPacklistPdf : Controller
    {

        private readonly ILogger<TourPdf> _logger;

        static private EasyjobDbContext _context;

        public TourPacklistPdf(EasyjobDbContext context, ILogger<TourPdf> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult CreateAllStockPdf(int id)
        {
            byte[] pdfBytes;


            var tourFromDb = _context.Tours.SingleOrDefault(x => x.IdTour == id);

            var idTour = new Microsoft.Data.SqlClient.SqlParameter()
            {
                ParameterName = "@IdTour",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = 10,
                Value = id
            };

            var allStock = _context.Stock2JobSPs.FromSqlRaw("exec expofair.CreatePacklistByTour @IdTour", idTour);
  
            using (var stream = new System.IO.MemoryStream())
            using (var wri = new PdfWriter(stream))
            using (var pdf = new PdfDocument(wri))
            using (var doc = new Document(pdf, PageSize.A4, false))
            {

                TableHeaderEventHandler handler = new TableHeaderEventHandler(doc, tourFromDb);
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

                // Calculate top margin to be sure that the table will fit the margin.
                float topMargin = 20 + handler.GetTableHeight();
                doc.SetMargins(topMargin, 20, 20, 20);


                if (!string.IsNullOrEmpty(tourFromDb.Comment))
                {

                    Table mainTable1 = new Table(UnitValue.CreatePercentArray(new float[] { 20, 60, 20 })).UseAllAvailableWidth();

                    Cell cell1 = getCell(1, 3, "").Add(formatContent(tourFromDb.Comment)).SetBorderBottom(Border.NO_BORDER);

                    mainTable1.AddCell(cell1);

                    cell1 = getCell(1, 3, "").Add(formatContent("Team: " + getStuffNames(tourFromDb.Team))).SetBorderBottom(Border.NO_BORDER);

                    mainTable1.AddCell(cell1);

                    doc.Add(mainTable1);

                }

                Table mainTable = new Table(UnitValue.CreatePercentArray(new float[] { 20, 60, 20 })).UseAllAvailableWidth();

                mainTable.AddHeaderCell(new Paragraph(new Text("Uhrzeit").SetBold()));

                Cell cellHeader = new Cell(1, 2);
                cellHeader.Add(new Paragraph(new Text("Packliste").SetBold()));
                mainTable.AddHeaderCell(cellHeader);

                Cell cellTime = new Cell().SetBorderBottom(Border.NO_BORDER); ;
                Cell cellContent = new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(5);
                Cell cellAdresse = new Cell().SetBorder(Border.NO_BORDER).SetBorderRight(new SolidBorder(ColorConstants.BLACK, 1));

                int cellColor = 0;

                Color greyColor = new DeviceRgb(224, 224, 224);

                foreach (var stock in allStock)
                {

                    cellTime = getCell(1, 1, "TIME");
                    cellContent = getCell(1, 1, "CONTENT");
                    cellAdresse = getCell(1, 1, "ADR");

                    if (cellColor == 1)
                    {
                        cellTime.SetBackgroundColor(greyColor);
                        cellContent.SetBackgroundColor(greyColor);
                        cellAdresse.SetBackgroundColor(greyColor);
                        cellColor = 0;
                    }
                    else
                    {
                        cellColor++;
                    }


                }

                Cell cellTableEnd = new Cell(1, 3).SetBorderTop(Border.NO_BORDER).Add(formatContent(""));
                mainTable.AddCell(cellTableEnd);

                doc.Add(mainTable);


                if (!string.IsNullOrEmpty(tourFromDb.Footer))
                {

                    Table mainTable1 = new Table(UnitValue.CreatePercentArray(new float[] { 20, 60, 20 })).UseAllAvailableWidth();

                    Cell cell1 = getCell(1, 3, "").Add(formatContent(tourFromDb.Footer));

                    mainTable1.AddCell(cell1);

                    doc.Add(mainTable1);

                }

                doc.Flush();
                doc.Close();
                pdfBytes = stream.ToArray();
            }
            return new FileContentResult(pdfBytes, "application/pdf");
        }

        static string getStuffNames(string StuffNumbers)
        {
            string StuffNames = "";
            if (StuffNumbers != null)
            {
                string[] numbers = StuffNumbers.Split(',');
                if (numbers != null)
                {
                    foreach (var num in numbers)
                    {
                        var employees = _context.Stuffs.Where(x => x.EmployeeNr == num).ToList();
                        foreach (var employee in employees)
                        {
                            StuffNames = StuffNames + employee.EmployeeName1 + ',';
                        }
                    }
                    StuffNames = StuffNames.Remove(StuffNames.Length - 1, 1);
                }
            }
            return StuffNames;
        }

        Paragraph formatContent(string content)
        {

            Paragraph para = new();

            string bold = "##"; // Bold
            string red = "**";  // Red

            bool containsBold = content.Contains(bold);
            bool containsRed = content.Contains(red);

            if (!containsBold && !containsRed)
            {
                para.Add(content);
                return para;
            }

            string[] boldSeparatingStrings = { "##" };
            string[] words = content.Split(boldSeparatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            bool isBold = content.StartsWith("##");

            foreach (var word in words)
            {
                if (isBold)
                {
                    // word is bold
                    para.Add(new Text(word).SetBold());
                    isBold = false;
                }
                else
                {
                    para.Add(word);
                    isBold = true;
                }
            }
            return para;
        }

        Cell getCell(int rowspan, int colspan, string type)
        {

            int defaultFontSize = 10;


            if (type == "TIME")
            {
                return new Cell().SetFontSize(defaultFontSize).SetBorder(Border.NO_BORDER).SetBorderLeft(new SolidBorder(ColorConstants.BLACK, 1)).SetBorderRight(new SolidBorder(ColorConstants.BLACK, 1));
            }
            else if (type == "CONTENT")
            {
                return new Cell().SetFontSize(defaultFontSize).SetBorder(Border.NO_BORDER).SetPaddingBottom(5);
            }
            else if (type == "ADR")
            {
                return new Cell().SetFontSize(defaultFontSize).SetBorder(Border.NO_BORDER).SetBorderRight(new SolidBorder(ColorConstants.BLACK, 1));
            }
            else
            {
                return new Cell(rowspan, colspan).SetFontSize(defaultFontSize);
            }
        }

        private class TableHeaderEventHandler : IEventHandler
        {
            private Table table;
            private float tableHeight;
            private Document doc;
            private Tour tour;

            public TableHeaderEventHandler(Document doc, Tour tour)
            {
                this.doc = doc;
                this.tour = tour;

                InitHeaderTable(tour);

                TableRenderer renderer = (TableRenderer)table.CreateRendererSubTree();
                renderer.SetParent(new DocumentRenderer(doc));

                // Simulate the positioning of the renderer to find out how much space the header table will occupy.
                LayoutResult result = renderer.Layout(new LayoutContext(new LayoutArea(0, PageSize.A4)));
                tableHeight = result.GetOccupiedArea().GetBBox().GetHeight();
            }

            public void HandleEvent(Event currentEvent)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
                PageSize pageSize = pdfDoc.GetDefaultPageSize();
                float coordX = pageSize.GetX() + doc.GetLeftMargin();
                float coordY = pageSize.GetTop() - doc.GetTopMargin();
                float width = pageSize.GetWidth() - doc.GetRightMargin() - doc.GetLeftMargin();
                float height = GetTableHeight();
                Rectangle rect = new Rectangle(coordX, coordY, width, height);

                new Canvas(canvas, rect)
                    .Add(table)
                    .Close();

            }


            public float GetTableHeight()
            {
                return tableHeight;
            }

            private void InitHeaderTable(Tour tour)
            {

                Table subTable = new Table(1).UseAllAvailableWidth();

                subTable.SetBorder(Border.NO_BORDER);

                Cell cell11 = new Cell().Add(new Paragraph("Fahrer").SetFontSize(6)).SetVerticalAlignment(VerticalAlignment.TOP).SetHorizontalAlignment(HorizontalAlignment.LEFT).SetBorder(Border.NO_BORDER); ;
                cell11.SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 1)).SetHeight(36);

                Div div1 = new Div().SetTextAlignment(TextAlignment.CENTER).SetPadding(0);
                div1.Add(new Paragraph(TourPacklistPdf.getStuffNames(tour.Driver)).SetFontSize(12));
                cell11.Add(div1);
                subTable.AddCell(cell11);

                Cell cell12 = new Cell().Add(new Paragraph("Verantwortlich").SetFontSize(6)).SetVerticalAlignment(VerticalAlignment.TOP).SetHorizontalAlignment(HorizontalAlignment.LEFT).SetBorder(Border.NO_BORDER).SetHeight(36);


                Div div2 = new Div().SetTextAlignment(TextAlignment.CENTER).SetPadding(0).SetVerticalAlignment(VerticalAlignment.MIDDLE).SetFontSize(10);
                div2.Add(new Paragraph(TourPacklistPdf.getStuffNames(tour.Master)).SetFontSize(12));
                cell12.Add(div2);
                subTable.AddCell(cell12);


                table = new Table(4).UseAllAvailableWidth();

                Cell cell1 = new Cell().Add(subTable);
                cell1.SetPadding(0);

                table.AddCell(cell1);

                Cell cell2 = new Cell().Add(new Paragraph("Datum").SetFontSize(6).SetVerticalAlignment(VerticalAlignment.TOP)).SetHorizontalAlignment(HorizontalAlignment.CENTER);
                cell2.Add(new Paragraph(tour.TourDate.ToString("dd.MM.yyyy")).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.BOTTOM).SetHeight(20));
                table.AddCell(cell2);

                var culture = new System.Globalization.CultureInfo("de-DE");

                Cell cell3 = new Cell().Add(new Paragraph("Wochentag").SetFontSize(6).SetVerticalAlignment(VerticalAlignment.TOP)).SetHorizontalAlignment(HorizontalAlignment.CENTER);
                cell3.Add(new Paragraph(culture.DateTimeFormat.GetDayName(tour.TourDate.DayOfWeek)).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.BOTTOM).SetHeight(20));
                table.AddCell(cell3);

                Cell cell4 = new Cell().Add(new Paragraph("LKW").SetFontSize(6).SetVerticalAlignment(VerticalAlignment.TOP)).SetHorizontalAlignment(HorizontalAlignment.CENTER);
                cell4.Add(new Paragraph(tour.VehicleNr).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.BOTTOM).SetHeight(15));
                table.AddCell(cell4);


            }
        }

    }

}
