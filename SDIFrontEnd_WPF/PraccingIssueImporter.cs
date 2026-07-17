using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using ITCLib;
using OpenXMLHelper;



namespace SDIFrontEnd_WPF
{
    //public interface IWordTableImporter
    //{
    //    void ImportData();
    //    void GetHeadings(); // record the indexes of the headings found in the document
    //}

    /// <summary>
    /// Class used to import Praccing Issues and Responses from a Word document.
    /// </summary>
    public class PraccingIssueImporter 
    {
        private string FilePath;
        WordprocessingDocument Document;

        Dictionary<string, int> Headings; // heading name, column index

        public List<Person> PeopleList;
        public List<PraccingCategory> CategoryList;

        public List<PraccingIssue> ImportedIssues;

        public List<StringPair> DatesToFix;
        List<StringPair> NamesToFix;
        public List<StringPair> Images;

        Survey TargetSurvey;

        double minuteOffset;
        // these 2 keep new issues and responses unique by assigning incremental IDs
        int issueIDOffset;
        int responseIDOffset;

        string lastIssueNo;

        public PraccingIssueImporter(Survey survey, string filePath)
        {
            TargetSurvey = survey;
            FilePath = filePath;

            Headings = new Dictionary<string, int>();

            ImportedIssues = new List<PraccingIssue>();

            DatesToFix = new List<StringPair>();
            NamesToFix = new List<StringPair>();
            Images = new List<StringPair>();
        }



        public void ImportData()
        {
            using (Document = WordprocessingDocument.Open(FilePath, false))
            {
                Body body = Document.MainDocumentPart.Document.Body;
                Table issuesTable = Document.MainDocumentPart.Document.Body.Elements<Table>().FirstOrDefault();

                if (issuesTable == null)
                    throw new Exception("Missing table.");

                GetHeadings();

                // check each date and from/to for valid entries, add invalid ones to a list
                foreach (TableRow row in issuesTable.Elements<TableRow>().Skip(1))
                {
                    if (InvalidDate(row))
                    {
                        throw new Exception("One or more issues / responses has a missing or invalid date. Please enter or fix the date and try again.\r\n" +
                            "Invalid date: " + row.Descendants<TableCell>().ElementAt(Headings["Date"]).GetCellText());
                    }

                }

                XMLUtilities.TagBold(body);
                XMLUtilities.TagItalics(body);
                XMLUtilities.TagHighlightingSpan(body);
                Images = ExtractImages(body, Document.MainDocumentPart);

                string issueNumber;
                string varname;

                foreach (TableRow row in issuesTable.Elements<TableRow>().Skip(1))
                {
                    var cells = row.Elements<TableCell>();

                    issueNumber = cells.ElementAt(Headings["IssueNo"]).GetCellText(); 
                    varname = cells.ElementAt(Headings["VarNames"]).GetCellText();

                    if (issueNumber.EndsWith("-1"))
                    {
                        AddMainIssue(row);
                        minuteOffset = 0;
                    }
                    else if (issueNumber.Contains("-"))
                    {
                        minuteOffset++;
                        AddResponse(row);

                    }
                    else if (!string.IsNullOrEmpty(varname) || issueNumber.ToLower().Contains("new"))
                    {
                        AddMainIssue(row);
                        minuteOffset = 0;
                    }
                    else if (string.IsNullOrEmpty(issueNumber))
                    {
                        minuteOffset++;
                        AddResponse(row, lastIssueNo);

                    }
                    else
                    {

                    }
                }
            }
        }

        public void GetHeadings()
        {
            var rows = Document.MainDocumentPart.Document.Body.Descendants<TableRow>();

            List<TableCell> headerCells = rows.ElementAt(0).Elements<TableCell>().ToList<TableCell>();

            for (int i = 0; i < headerCells.Count(); i++)
            {
                string cellText = headerCells.ElementAt(i).GetCellText();
                int newLine = cellText.IndexOf("\r");
                if (newLine > -1)
                    cellText = cellText.Substring(0, cellText.IndexOf("\r"));


                switch (cellText)
                {
                    case "#":
                        Headings.Add("IssueNo", i);
                        break;
                    case "Var Name":
                        Headings.Add("VarNames", i);
                        break;
                    case "Description/Response":
                        Headings.Add("Description", i);
                        break;
                    case "Date":
                        Headings.Add("Date", i);
                        break;
                    case "From":
                        Headings.Add("From", i);
                        break;
                    case "To":
                        Headings.Add("To", i);
                        break;
                    case "Category":
                        Headings.Add("Category", i);
                        break;
                    case "PIN":
                        Headings.Add("PIN", i);
                        break;
                }
            }
        }

        private void CheckValidDates(TableRow row)
        {
            var cells = row.Elements<TableCell>();

            string date = cells.ElementAt(Headings["Date"]).GetCellText();
            if (string.IsNullOrEmpty(date))
                return;

            if (!DateTime.TryParse(date, out DateTime d))
            {
                if (!DatesToFix.Any(x => x.String1.Equals(date))) DatesToFix.Add(new StringPair(date));
            }

            // check if the date is before a DateTimePicker's minimum date
            if (d < new DateTime(1753,1,1))
                DatesToFix.Add(new StringPair(date));
        }

        private void CheckValidNames(TableRow row)
        {
            //var cells = row.Elements<TableCell>();
            //string from = Utilities.TrimString(cells.ElementAt(Headings["Date"]).GetCellText(), " ");
            //if (!string.IsNullOrEmpty(from) && personList.Where(x => x.Name.Equals(from)).FirstOrDefault() == null)
            //    NamesToFix.Add(new StringPair(from));

            //string to = Utilities.TrimString(cells.ElementAt(Headings["To"]).GetCellText(), " ");
            //if (!string.IsNullOrEmpty(to) && personList.Where(x => x.Name.Equals(to)).FirstOrDefault() == null)
            //    NamesToFix.Add(new StringPair(to));
        }

        private bool MissingDates(TableRow row)
        {
            var cells = row.Elements<TableCell>();

            string date = cells.ElementAt(Headings["Date"]).GetCellText();
            string desc = cells.ElementAt(Headings["Description"]).GetCellText();
            if (string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(desc))
                return true;

            return false;
        }

        private bool InvalidDate(TableRow row)
        {
            var cells = row.Elements<TableCell>();

            string date = cells.ElementAt(Headings["Date"]).GetCellText();
            string desc = cells.ElementAt(Headings["Description"]).GetCellText();

            if (string.IsNullOrEmpty(desc))
                return false;

            // invalid format
            if (!DateTime.TryParse(date, out DateTime d))
                return true;
            // out of range
            if (d < new DateTime(1753, 1, 1))
                return true;
            // empty date
            if (string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(desc))
                return true;

            return false;
        }

        /// <summary>
        /// Saves all images in the document to the application's folder and returns a StringPair list of their Ids and names.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="wDoc"></param>
        /// <returns></returns>
        private List<StringPair> ExtractImages(Body content, MainDocumentPart wDoc)
        {
            List<StringPair> imageList = new List<StringPair>();

            foreach (Run run in content.Descendants<Run>())
            {
                //detect if the run contains an image and upload it to wordpress
                Drawing image = run.Descendants<Drawing>().FirstOrDefault();

                if (image != null)
                {
                    DocumentFormat.OpenXml.Drawing.Pictures.Picture imageFirst;
                    if (image.Inline == null)
                        imageFirst = image.Anchor.Descendants<DocumentFormat.OpenXml.Drawing.Graphic>().FirstOrDefault().GraphicData.Descendants<DocumentFormat.OpenXml.Drawing.Pictures.Picture>().FirstOrDefault();
                    else
                        imageFirst = image.Inline.Graphic.GraphicData.Descendants<DocumentFormat.OpenXml.Drawing.Pictures.Picture>().FirstOrDefault();

                    if (imageFirst == null)
                        continue;

                    var blip = imageFirst.BlipFill.Blip.Embed.Value;

                    if (imageList.Any(x => x.String1.Equals(blip)))
                        continue;

                    ImagePart img = (ImagePart)wDoc.Document.MainDocumentPart.GetPartById(blip);
                    string imageFileName = string.Empty;

                    //the image is stored in a zip file code behind, so it must be extracted

                    using (Image toSaveImage = Image.FromStream(img.GetStream()))
                    {
                        imageFileName = "Praccing Image - " + TargetSurvey.SurveyCode + " - " + DateTime.Now.Month.ToString().Trim() +
                        DateTime.Now.Day.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() +
                        DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + ".png";

                        try
                        {
                            //toSaveImage.Save(AppImageRepo + @"\" + imageFileName);
                            toSaveImage.Save(imageFileName);
                        }
                        catch (Exception)
                        {

                        }
                    }

                    StringPair sp = new StringPair(blip, imageFileName);

                    imageList.Add(sp);
                }
            }

            return imageList;
        }

        private void AddMainIssue(TableRow row)
        {
            var cells = row.Elements<TableCell>();

            string varNames;
            string description;
            string descriptionRTF;
            string date;
            string from;
            string to;
            string category;
            string issueNumber = cells.ElementAt(Headings["IssueNo"]).GetCellText();
            string pin;
            int issueNo = 0;

            if (string.IsNullOrEmpty(issueNumber) || issueNumber.ToLower().Contains("new"))
            {
                issueNo = -1 - issueIDOffset;
                issueIDOffset++;
                lastIssueNo = issueNo + "-1";
            }
            else
            {
                issueNo = Int32.Parse(issueNumber.Substring(0, issueNumber.IndexOf("-")));
                lastIssueNo = issueNumber;
            }

            

            varNames = Utilities.RemoveTags(cells.ElementAt(Headings["VarNames"]).GetCellText());
            description = cells.ElementAt(Headings["Description"]).GetCellText().Trim();
            description = description.TrimAndRemoveAll("<br>");
            descriptionRTF = description;

            // check date and names for valid entries
            date = Utilities.RemoveTags(cells.ElementAt(Headings["Date"]).GetCellText());
            date = date.Trim(new char[] { '\r', '\n' });
            DateTime issueDate = GetIssueDate(date);

            from = Utilities.RemoveTags(cells.ElementAt(Headings["From"]).GetCellText());
            from = from.Trim(new char[] { '\r', '\n' });
            Person issueFrom = PeopleList.FirstOrDefault(x => x.Name.Equals(from)) ?? new Person(from, 0); ;
            

            to = Utilities.RemoveTags(cells.ElementAt(Headings["To"]).GetCellText());
            to = to.Trim(new char[] { '\r', '\n' });
            Person issueTo = PeopleList.FirstOrDefault(x => x.Name.Equals(to)) ?? new Person(to, 0);

            pin = Utilities.RemoveTags(cells.ElementAt(Headings["PIN"]).GetCellText());
            pin = pin.Trim(new char[] { '\r', '\n' });

            category = Utilities.RemoveTags(cells.ElementAt(Headings["Category"]).GetCellText());
            category = category.Trim(new char[] { '\r', '\n' });
            PraccingCategory issueCategory = CategoryList.FirstOrDefault(x => x.Category.Equals(category)) ?? new PraccingCategory();

            // images
            List<PraccingImage> issueImages = new List<PraccingImage>();
            var imageRuns = cells.ElementAt(Headings["Description"]).Descendants<Run>();
            if (imageRuns.Any(x => x.Descendants<Drawing>().Count() > 0))
            {
                foreach (Run r in imageRuns)
                {
                    Drawing image = r.Descendants<Drawing>().FirstOrDefault();

                    if (image != null)
                    {
                        DocumentFormat.OpenXml.Drawing.Pictures.Picture imageFirst;
                        if (image.Inline == null)
                            imageFirst = image.Anchor.Descendants<DocumentFormat.OpenXml.Drawing.Graphic>().FirstOrDefault().GraphicData.Descendants<DocumentFormat.OpenXml.Drawing.Pictures.Picture>().FirstOrDefault();
                        else
                            imageFirst = image.Inline.Graphic.GraphicData.Descendants<DocumentFormat.OpenXml.Drawing.Pictures.Picture>().FirstOrDefault();

                        var blip = imageFirst.BlipFill.Blip.Embed.Value;
                        foreach (StringPair sp in Images)
                        {
                            if (sp.String1 == blip)
                                issueImages.Add(new PraccingImage(0, sp.String2));
                        }

                    }
                }
            }

            PraccingIssue mainIssue = new PraccingIssue();
            mainIssue.Survey.SID = TargetSurvey.SID;
            mainIssue.Survey.SurveyCode = TargetSurvey.SurveyCode;
            mainIssue.IssueNo = issueNo;
            mainIssue.VarNames = varNames;
            mainIssue.Description = description;
            mainIssue.IssueDate = issueDate;
            mainIssue.IssueFrom = issueFrom;
            mainIssue.IssueTo = issueTo;
            mainIssue.Category = issueCategory;
            mainIssue.PinNo = pin;
            mainIssue.Images = issueImages;

            ImportedIssues.Add(mainIssue);
        }

        private void AddResponse(TableRow row, string lastIssueNo = "")
        {
            var cells = row.Elements<TableCell>();

            string description;
            string date;
            string from;
            string to;
            string issueNumber;
            string pin;

            if (string.IsNullOrEmpty(lastIssueNo))
                issueNumber = cells.ElementAt(Headings["IssueNo"]).GetCellText();
            else
                issueNumber = lastIssueNo;

            int issueNo;
            issueNo = Int32.Parse(issueNumber.Substring(0, issueNumber.IndexOf("-", 1)));

            description = cells.ElementAt(Headings["Description"]).GetCellText();
            description = description.Trim(new char[] { ' ' });
            description = description.TrimAndRemoveAll("<br>");

            string forcompare = Utilities.PrepareTextCompare(description);

            if (string.IsNullOrEmpty(description))
                return;

            int responseID = 0;
            //PraccingIssue matchingIssue = ReferenceList.FirstOrDefault(x => x.Survey.SurveyCode.Equals(TargetSurvey.SurveyCode) && x.IssueNo == issueNo);
            //if (matchingIssue != null)
            //{
            //    PraccingResponse matchingResponse = matchingIssue.Responses.FirstOrDefault(x => Utilities.PrepareTextCompare(x.Response).Equals(forcompare));

            //    if (matchingResponse != null)
            //        responseID = matchingResponse.ID;
            //    else
            //        responseID = 0;
            //}
            //else
            //{
            //    responseID = 0;
            //}

            // if the ID is not 0, this response already exists and should be already part of the main issue
            // only continue if this is a new response
            if (responseID != 0)
            {

            }
            else
            {
                responseID = -1 - responseIDOffset;
                responseIDOffset++;
            }

            date = cells.ElementAt(Headings["Date"]).GetCellText();
            date = date.Trim(new char[] { '\r', '\n' });
            DateTime issueDate = GetResponseDate(date);


            from = cells.ElementAt(Headings["From"]).GetCellText();
            from = from.Trim(new char[] { '\r', '\n' });
            Person issueFrom = PeopleList.FirstOrDefault(x => x.Name.Equals(from)) ?? new Person(from, 0); ;

            to = cells.ElementAt(Headings["To"]).GetCellText();
            to = to.Trim(new char[] { '\r', '\n' });
            Person issueTo = PeopleList.FirstOrDefault(x => x.Name.Equals(to)) ?? new Person(to, 0); ;

            pin = cells.ElementAt(Headings["PIN"]).GetCellText();
            pin = pin.Trim(new char[] { '\r', '\n' });

            // images
            List<PraccingImage> issueImages = new List<PraccingImage>();
            var imageRuns = cells.ElementAt(Headings["Description"]).Descendants<Run>();
            if (imageRuns.Any(x => x.Descendants<Drawing>().Count() > 0))
            {
                foreach (Run r in imageRuns)
                {
                    Drawing image = r.Descendants<Drawing>().FirstOrDefault();

                    if (image != null)
                    {

                        DocumentFormat.OpenXml.Drawing.Pictures.Picture imageFirst;
                        if (image.Inline == null)
                            imageFirst = image.Anchor.Descendants<DocumentFormat.OpenXml.Drawing.Graphic>().FirstOrDefault().GraphicData.Descendants<DocumentFormat.OpenXml.Drawing.Pictures.Picture>().FirstOrDefault();
                        else
                            imageFirst = image.Inline.Graphic.GraphicData.Descendants<DocumentFormat.OpenXml.Drawing.Pictures.Picture>().FirstOrDefault();

                        if (imageFirst == null)
                            continue;

                        var blip = imageFirst.BlipFill.Blip.Embed.Value;
                        foreach (StringPair sp in Images)
                        {
                            if (sp.String1 == blip)
                                issueImages.Add(new PraccingImage(0, sp.String2));
                        }

                    }
                }
            }

            PraccingResponse response = new PraccingResponse();
            response.ID = responseID;
            response.IssueID = (ImportedIssues.Where(x => x.IssueNo == issueNo)).FirstOrDefault().ID;

            response.Response = description;
            response.ResponseDate = issueDate;
            response.ResponseFrom = issueFrom;
            response.ResponseTo = issueTo;
            response.PinNo = pin;
            response.Images = issueImages;

            // find which main issue this response belongs to
            var mainIssue = ImportedIssues.Where(x => x.IssueNo == issueNo).FirstOrDefault();


            mainIssue.Responses.Add(response);

        }

        private DateTime GetIssueDate(string date)
        {
            if (!DateTime.TryParse(date, out DateTime issueDate))
            {
                var found = DatesToFix.Where(x => x.String1.Equals(date)).FirstOrDefault();
                if (found != null)
                    issueDate = DateTime.Parse(found.String2);
            }

            return issueDate;
        }

        private DateTime GetResponseDate(string date)
        {
            if (!DateTime.TryParse(date, out DateTime responseDate))
            {
                var found = DatesToFix.Where(x => x.String1.Equals(date)).FirstOrDefault();
                if (found != null)
                {
                    responseDate = DateTime.Parse(found.String2);

                    responseDate += DateTime.Now.TimeOfDay;
                    responseDate.AddMinutes(minuteOffset);
                }
            }
            else
            {
                responseDate = DateTime.Parse(date);
                responseDate += DateTime.Now.TimeOfDay;
                responseDate.AddMinutes(minuteOffset);
            }

            return responseDate;
        }
    }

    
}
