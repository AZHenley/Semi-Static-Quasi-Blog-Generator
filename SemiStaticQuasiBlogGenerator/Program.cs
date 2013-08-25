using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownSharp;

namespace SemiStaticQuasiBlogGenerator
{
    class Program
    {
        static Markdown markdown = new Markdown();

        static void Main(string[] args)
        {
            //get all the directories
            string rootDir; string contentDir; string pagesDir; string postsDir; string outputDir;
            if (Directory.Exists(@"C:\Users\azhenley\"))
                rootDir = @"C:\Users\azhenley\Dropbox\SemiStaticQuasiBlogGenerator\Site\"; //work desktop
            else
                rootDir = @"C:\Users\Austin\Dropbox\SemiStaticQuasiBlogGenerator\Site\"; //home desktop and laptop
            contentDir = rootDir + @"content\";
            pagesDir = contentDir + @"pages\";
            postsDir = contentDir + @"posts\";
            outputDir = rootDir + @"output\";
            Empty(outputDir); //clean up

            //get all the content
            string[] pagesPaths = Directory.GetFiles(pagesDir, "*.txt");
            string[] postsPaths = Directory.GetFiles(postsDir, "*.txt");
            List<Content> pages = new List<Content>();
            List<Content> posts = new List<Content>();
            foreach (string path in pagesPaths)
            {
                pages.Add(new Content(path, true));
            }
            foreach (string path in postsPaths)
            {
                posts.Add(new Content(path));
            }
            pages.Sort((a, b) => DateTime.Parse(a.date).CompareTo(DateTime.Parse(b.date)));
            posts.Sort((a, b) => DateTime.Parse(a.date).CompareTo(DateTime.Parse(b.date)));

            //generate the common pieces of HTML
            string topHTML = GenerateTop(pages);
            string leftHTML = GenerateLeft(posts);
            string fragHTML = File.ReadAllText(rootDir + "frag.html").Replace("<div class=\"abc0\"></div>", topHTML).Replace("<div class=\"abc1\"></div>", leftHTML);

            //generate the posts and pages
            foreach (Content p in pages.Concat(posts))
            {
                string tmpOutput = fragHTML.Replace("<div class=\"abc2\"></div>", p.output);
                if (p.isPage)
                {
                    int start = tmpOutput.IndexOf("<div class=\"r1\"></div>");
                    int end = tmpOutput.IndexOf("<div class=\"r2\"></div>") + 24;
                    tmpOutput = tmpOutput.Remove(start, end - start);
                }

                File.WriteAllText(outputDir + p.fileName, tmpOutput);
            }

            Console.WriteLine("Done.");
        }

        static string GenerateTop(List<Content> pages)
        {
            string topHTML = "";
            foreach (Content page in pages)
            {
                topHTML += "<li><a href=\"" + page.fileName + "\">" + page.title + "</a></li>";
            }

            return topHTML;
        }

        static string GenerateLeft(List<Content> posts)
        {
            string leftHTML = "";
            foreach (Content post in posts)
            {
                leftHTML += "<a href=\"" + post.fileName + "\">" + post.title + "</a>";
            }

            return leftHTML;
        }

        static void Empty(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }

    class Content
    {
        static Markdown markdown = new Markdown();

        public bool isPage { get; set; }
        public string path { get; set; }
        public string fileName { get; set; }
        public string title { get; set; }
        public string date { get; set; }
        public string contents { get; set; }
        public string output { get; set; }

        public Content(string filePath, bool isThisAPage=false)
        {
            markdown = new Markdown();

            isPage = isThisAPage;
            path = filePath;
            fileName = StripNameFromPath(path) + ".html";
            string[] lines = File.ReadAllLines(path);
            title = lines[0].Replace("title:", "").Trim();
            date = lines[1].Replace("date:", "").Trim();
            contents = string.Join(Environment.NewLine, lines.Skip(2));
            if (fileName.Contains("index"))
                output = markdown.Transform(contents);
            else if (isPage)
                output = "<h2 class=\"page-header\">" + title + "</h2>" + markdown.Transform(contents);
            else //posts
                output = "<h2 class=\"page-header\">" + title + "</h2><p><small>" + date + "</small></p>" + markdown.Transform(contents);
        }

        string StripNameFromPath(string path)
        {
            return path.Split('\\').Last().Split('.')[0];
        }
    }
}