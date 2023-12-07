using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Grid = Microsoft.Maui.Controls.Grid;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace Excel
{
    public class Query
    {
        public string name { get; set; }
        public string info { get; set; }
        public string subject { get; set; }
        public string year { get; set; }
        public string author { get; set; }
        public Query()
        {
            name = "";
            subject = "";
            year = "";
            author = "";
        }
        public void output()
        {
            System.Diagnostics.Debug.WriteLine(name);
            System.Diagnostics.Debug.WriteLine(subject);
            System.Diagnostics.Debug.WriteLine(year);
            System.Diagnostics.Debug.WriteLine(author);
            System.Diagnostics.Debug.WriteLine("");
        }
    }
    public class Book:INotifyPropertyChanged
    {
        private string _name;
        public string name
        {
            get { return _name; }
            set 
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        private string _info;
        public string info
        {
            get { return _info; }
            set 
            { 
                _info = value;
                OnPropertyChanged();
            }
        }
        private string _subject;
        public string subject
        {
            get { return _subject; }
            set 
            { 
                _subject = value;
                OnPropertyChanged();
            }
        }
        private string _year;
        public string year
        {
            get { return _year; }
            set 
            { 
                _year = value;
                OnPropertyChanged();
            }
        }

        private string _authors;
        public string authors
        {
            get { return _authors; }
            set
            { 
                _authors = value;
                OnPropertyChanged();
            }
        }
        public void output()
        {
            System.Diagnostics.Debug.WriteLine(name);
            System.Diagnostics.Debug.WriteLine(info);
            System.Diagnostics.Debug.WriteLine(subject);
            System.Diagnostics.Debug.WriteLine(year);
            System.Diagnostics.Debug.Write(authors);
            System.Diagnostics.Debug.WriteLine("");
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public interface IParser
    {
        IList<Book> books { get; }
        void QueryBooks(Query query, string filePath);
        void HttpTransform(string XmlFile, string XslFile, string HtmlFile);
    }
    public class DomParser : IParser
    {
        public IList<Book> books { get; }
        public DomParser()
        {
            books = new List<Book>();
        }
        private static void RecurseNodes(int start, XmlNode node, Book book)
        {
            var sb = new StringBuilder();
            RecurseNodes(node, start, sb, book);
            Console.WriteLine(sb.ToString());
        }
        private static void RecurseNodes(XmlNode node, int level, StringBuilder sb, Book book)
        {
            if (node == null)
            {
                Console.WriteLine("No matching elements found");
                return;
            }
            if (level == 1) sb.AppendFormat("\n");
            if (level > 0) sb.AppendFormat("{0}\nType:{1}\nName:{2}\nAttributes:",
            level, node.NodeType, node.Name);
            else sb.AppendFormat("{0}\nType:{1}\nName:{2}",
            level, node.NodeType, node.Name);
            if (level > 0) foreach (XmlAttribute attr in node.Attributes)
                {
                    if (level == 1 && attr.Name == "BK_NAME") book.name = attr.Value;
                    if (level == 1 && attr.Name == "BK_INFO") book.info = attr.Value;
                    if (level == 1 && attr.Name == "DC_NAME") book.subject = attr.Value;
                    if (level == 1 && attr.Name == "YEAR") book.year = attr.Value;
                    if (level == 2 && attr.Name == "AU_NAME") book.authors+=(attr.Value+" ");
                    sb.AppendFormat("\n{0}={1}", attr.Name, attr.Value);
                }
            sb.AppendLine();
            foreach (XmlNode n in node.ChildNodes)
            {
                RecurseNodes(n, level + 1, sb,book);
            }
        }
        public void QueryBooks(Query q,string s)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(GetFilePath(s));
            var xpathExpression = "//book" +
                                  (string.IsNullOrEmpty(q.name) ? "" : $"[@BK_NAME='{q.name}']") +
                                  (string.IsNullOrEmpty(q.subject) ? "" : $"[@DC_NAME='{q.subject}']") +
                                  (string.IsNullOrEmpty(q.year) ? "" : $"[@YEAR='{q.year}']") +
                                  (string.IsNullOrEmpty(q.author) ? "" : $"[author[@AU_NAME='{q.author}']]");
            var nodes = xmlDoc.SelectNodes(xpathExpression);
            if (nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    Book b = new Book();
                    RecurseNodes(1, node,b);
                    books.Add(b);
                }
            }
            else
            {
                Console.WriteLine("No matching elements found");
            }
        }
        public void HttpTransform(string XmlFile, string XslFile, string HtmlFile)
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            string f1 = GetFilePath(XslFile);
            xslt.Load(f1);
            string f2 = GetFilePath(XmlFile);
            string f3 = GetFilePath(HtmlFile);
            xslt.Transform(f2, f3);
        }
        private static string GetFilePath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        }
    }
    public class SaxParser: IParser
    {
        public IList<Book> books { get; } //= new List<Book>();
        public SaxParser()
        {
            books = new List<Book>();
        }
        private static string GetFilePath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        }
        public void QueryBooks(Query q,string s)
        {
            Book b = new Book();
            var xmlReader = new XmlTextReader(GetFilePath(s));
            bool isInsideBook = false;
            bool check = false;
            string currentName = "";
            List<string> authors = new List<string>();
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xmlReader.Name == "book")
                        {
                            isInsideBook = true;
                            currentName = Convert.ToString(xmlReader.GetAttribute("BK_NAME"));
                            authors.Clear();
                            bool yearMatch = string.IsNullOrEmpty(q.year) ||
                                             (int.TryParse(xmlReader.GetAttribute("YEAR"), out int bookYear) && int.TryParse(q.year, out int queryYear) && bookYear == queryYear);
                            if ((currentName == q.name || string.IsNullOrEmpty(q.name)) &&
                                (xmlReader.GetAttribute("DC_NAME") == q.subject || string.IsNullOrEmpty(q.subject)) &&
                                yearMatch &&
                                (string.IsNullOrEmpty(xmlReader.GetAttribute("YEAR")) || yearMatch))
                            {
                                books.Add(b);
                                b = new Book();
                                b.name= Convert.ToString(xmlReader.GetAttribute("BK_NAME"));
                                b.subject = Convert.ToString(xmlReader.GetAttribute("DC_NAME"));
                                b.year = Convert.ToString(xmlReader.GetAttribute("YEAR"));
                                b.info = Convert.ToString(xmlReader.GetAttribute("BK_INFO"));
                                check = true;
                                Console.WriteLine("Element: {0}", xmlReader.Name);
                                PrintAttributes(xmlReader);
                            }
                            else
                            {
                                isInsideBook = false;
                            }
                        }
                        else if (isInsideBook && xmlReader.Name == "author")
                        {
                            authors.Add(Convert.ToString(xmlReader.GetAttribute("AU_NAME")));
                            b.authors+=(Convert.ToString(xmlReader.GetAttribute("AU_NAME"))+" ");
                        }
                        break;
                    case XmlNodeType.Text:
                        if (isInsideBook)
                        {
                            Console.WriteLine("- Value: {0}", xmlReader.Value);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (isInsideBook && xmlReader.Name == "book")
                        {
                            isInsideBook = false;
                            if (authors.Any(a => a == q.author) || string.IsNullOrEmpty(q.author))
                            {
                                Console.WriteLine("Authors: {0}", string.Join(" ", authors));
                            }
                        }
                        break;
                }
            }
            books.Add(b);
            xmlReader.Close();
        }
        private static void PrintAttributes(XmlTextReader xmlReader)
        {
            if (xmlReader.HasAttributes)
            {
                while (xmlReader.MoveToNextAttribute())
                {
                    Console.WriteLine("- Attribute: {0} = {1}", xmlReader.Name, xmlReader.Value);
                }
            }
        }
        public void HttpTransform(string XmlFile, string XslFile, string HtmlFile)
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            string f1 = GetFilePath(XslFile);
            xslt.Load(f1);
            string f2 = GetFilePath(XmlFile);
            string f3 = GetFilePath(HtmlFile);
            xslt.Transform(f2, f3);
        }
    }
    public class LinqParser: IParser
    {
        public IList<Book> books { get; } //= new List<Book>();
        public LinqParser()
        {
            books = new List<Book>();
        }
        private static string GetFilePath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        }
        public void QueryBooks(Query q,string s)
        {
            var doc = XDocument.Load(GetFilePath(s));
            var result = (from book in doc.Descendants("book")
                          where (string.IsNullOrEmpty(q.name) || (string)book.Attribute("BK_NAME") == q.name) &&
                                (string.IsNullOrEmpty(q.subject) || (string)book.Attribute("DC_NAME") == q.subject) &&
                                (string.IsNullOrEmpty(q.year) || (int)book.Attribute("YEAR") == int.Parse(q.year)) &&
                                (string.IsNullOrEmpty(q.author) || book.Descendants("author").Any(author => (string)author.Attribute("AU_NAME") == q.author))
                          select new
                          {
                              bk_id = (int)book.Attribute("BK_ID"),
                              bk_name = (string)book.Attribute("BK_NAME"),
                              bk_info = (string)book.Attribute("BK_INFO"),
                              bk_dc = (int)book.Attribute("BK_DC"),
                              bk_year = (int)book.Attribute("YEAR"),
                              dc_name = (string)book.Attribute("DC_NAME"),
                              authors = book.Descendants("author").Select(author => (string)author.Attribute("AU_NAME")).ToList()
                          }).Distinct().ToList();
            if (result.Count > 0)
            {
                foreach (var b in result)
                {
                    Book bk = new Book();
                    bk.name = b.bk_name;
                    bk.subject = b.dc_name;
                    bk.info = b.bk_info;
                    bk.year = b.bk_year.ToString();
                    for (int i = 0; i < b.authors.Count();++i) bk.authors += (b.authors[i] + " ");
                    books.Add(bk);
                    Console.WriteLine($"BK_ID:{b.bk_id}\nBK_NAME:{b.bk_name}\nBK_INFO:{b.bk_info}\nYEAR:{b.bk_year}\nDC_NAME:{b.dc_name}\nAuthors: {string.Join(", ", b.authors)}\n");
                }
            }
            else
            {
                Console.WriteLine("No matching elements found");
            }
        }
        public void HttpTransform(string XmlFile,string XslFile,string HtmlFile)
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            string f1 = GetFilePath(XslFile);
            xslt.Load(f1);
            string f2 = GetFilePath(XmlFile);
            string f3 = GetFilePath(HtmlFile);
            xslt.Transform(f2, f3);
        }
    }
    public partial class MainPage : ContentPage
    {
        private IParser bookParser;
        Query q=new Query();
        IList<Book> b = new List<Book>();
        string selectedFileXMLPath="";
        string selectedFileXSLPath="";
        string selectedFileHTMLPath="";
        public MainPage()
        {
            InitializeComponent();
        }
        private async void SelectXMLButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        {DevicePlatform.iOS, new[] {"public.xml"} },
                        {DevicePlatform.Android, new[] {"application/xml"} },
                        {DevicePlatform.WinUI, new[] {".xml"} }
                    }),

                });
                if(fileResult !=null)
                {
                    selectedFileXMLPath = fileResult.FullPath;
                    System.Diagnostics.Debug.WriteLine(selectedFileXMLPath);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void SelectXSLButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        {DevicePlatform.iOS, new[] {"public.xsl"} },
                        {DevicePlatform.Android, new[] {"application/xsl"} },
                        {DevicePlatform.WinUI, new[] {".xsl"} }
                    }),

                });
                if (fileResult != null)
                {
                    selectedFileXSLPath = fileResult.FullPath;
                    System.Diagnostics.Debug.WriteLine(selectedFileXSLPath);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void SelectHTMLButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        {DevicePlatform.iOS, new[] {"public.html"} },
                        {DevicePlatform.Android, new[] {"application/html"} },
                        {DevicePlatform.WinUI, new[] {".html"} }
                    }),

                });
                if (fileResult != null)
                {
                    selectedFileHTMLPath = fileResult.FullPath;
                    System.Diagnostics.Debug.WriteLine(selectedFileHTMLPath);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void TransformButtonClicked(object sender, EventArgs e)
        {
            if (selectedFileXMLPath == "")
            {
                Error(1);
                return;
            }
            if (selectedFileXSLPath=="")
            {
                Error(2);
                return;
            }
            if (selectedFileHTMLPath == "")
            {
                Error(3);
                return;
            }
            if(bookParser==null)
            {
                Error(4);
                return;
            }
            bookParser.HttpTransform(selectedFileXMLPath, selectedFileXSLPath, selectedFileHTMLPath);
        }
        private void SetParserStrategy(string method)
        {
            if (method == "DOM") bookParser = new DomParser();
            else if (method == "SAX") bookParser = new SaxParser();
            else if (method == "LINQ") bookParser = new LinqParser();                
        }
        private async void MakeQueryButtonClicked(object sender, EventArgs e)
        {
            q.name = $"{NamePicker.SelectedItem}";
            q.subject = $"{SubjectPicker.SelectedItem}";
            q.year = $"{YearPicker.SelectedItem}";
            q.author = $"{AuthorPicker.SelectedItem}";
            QueryOutput(q);
        }
        private async void AnalyzeButtonClicked(object sender, EventArgs e)
        {
            if (selectedFileXMLPath == "")
            {
                Error(1);
                return;
            }    
            string method = $"{MethodPicker.SelectedItem}";
            if (method != "SAX" && method != "DOM" && method != "LINQ")
            {
                Error(4);
                return;
            }
            SetParserStrategy(method);
            bookParser.books.Clear();
            bookParser.QueryBooks(q,selectedFileXMLPath);
            Results.ItemsSource = null;
            Results.ItemsSource = bookParser.books;
        }
        private async void NameCheckChanged(object sender, EventArgs e)
        {
            if (NameFilter.IsChecked) NamePicker.IsVisible = true;
            else NamePicker.IsVisible = false;
        }
        private async void SubjectCheckChanged(object sender, EventArgs e)
        {
            if (SubjectFilter.IsChecked) SubjectPicker.IsVisible = true;
            else SubjectPicker.IsVisible = false;
        }
        private async void YearCheckChanged(object sender, EventArgs e)
        {
            if (YearFilter.IsChecked) YearPicker.IsVisible = true;
            else YearPicker.IsVisible = false;
        }
        private void AuthorCheckChanged(object sender, EventArgs e)
        {
            if (AuthorFilter.IsChecked) AuthorPicker.IsVisible = true;
            else AuthorPicker.IsVisible = false;
        }
        private async void ClearButtonClicked(object sender, EventArgs e)
        {
            bookParser.books.Clear();
            Results.ItemsSource = null;
            NamePicker.SelectedItem = null;
            YearPicker.SelectedItem = null;
            SubjectPicker.SelectedItem = null;
            AuthorPicker.SelectedItem = null;
            MethodPicker.SelectedItem = null;
            NameFilter.IsChecked = false;
            YearFilter.IsChecked = false;
            SubjectFilter.IsChecked = false;
            AuthorFilter.IsChecked = false;
        }
        private async void Error(int type)
        {
            if(type==1) await DisplayAlert("Помилка", "XML-файл не вибрано", "OK");
            if (type == 2) await DisplayAlert("Помилка", "XSL-файл не вибрано", "OK");
            if (type == 3) await DisplayAlert("Помилка", "HTML-файл не вибрано", "OK");
            if (type == 4) await DisplayAlert("Помилка", "Тип обробки не вибрано", "OK");
        }
        private async void QueryOutput(Query q)
        {
            string s="Назва книжки - ";
            if (q.name == "") s += "довiльна\n";
            else s+=(q.name+'\n');
            s += "Предмет - ";
            if (q.subject == "") s += "довiльний\n";
            else s += (q.subject + '\n');
            s += "Рік видання - ";
            if (q.year == "") s += "довiльний\n";
            else s += (q.year + '\n');
            s += "Автор - ";
            if (q.author == "") s += "довiльний\n";
            else s += (q.author + '\n');
            await DisplayAlert("Сформований запит",s, "OK");
            
        }
        private async void ExitButtonClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Пiдтвердження", "Ви дiйсно хочете вийти ? ", "Так", "Нi");
            if (answer) System.Environment.Exit(0);
        }
        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка", "Лабораторна робота 2 студентки Багнюк Ангеліни. Програма для роботи з XML-файлом, що містить інформацію про книжки, та пошуку книжок за потрібними критеріями", "OK");
        }
    }
}