using System.Collections.ObjectModel;
using System.IO;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class ProjectManager
    {
        public string? CurrentProjectPath { get; private set; }

        public string? CurrentProjectName =>
            string.IsNullOrEmpty(CurrentProjectPath)
                ? null
                : Path.GetFileName(
                    CurrentProjectPath.TrimEnd(
                        Path.DirectorySeparatorChar
                    )
                );

        public ObservableCollection<ProjectItem> ProjectItems { get; } = new();

        public bool IsProjectOpen =>
            !string.IsNullOrEmpty(CurrentProjectPath);

        public void OpenProject(string projectPath)
        {
            if (!Directory.Exists(projectPath))
                return;

            CurrentProjectPath = projectPath;

            LoadProjectItems();
        }

        public string? CreateProject(
             ProjectCreationOptions options)
        {
            if (!Directory.Exists(options.ParentDirectory))
                return null;

            if (string.IsNullOrWhiteSpace(options.ProjectName))
                return null;

            string projectPath = Path.Combine(
                options.ParentDirectory,
                options.ProjectName
            );

            if (Directory.Exists(projectPath))
                return null;

            try
            {
                Directory.CreateDirectory(
                    projectPath
                );

                switch (options.ProjectType)
                {
                    case ProjectType.Html:
                    case ProjectType.Php:
                        CreateProjectFiles(
                            projectPath,
                            options.ProjectName,
                            options.ProjectType
                        );
                        break;

                    case ProjectType.CodeIgniter:
                        CreatePhpProject(
                            projectPath,
                            options.ProjectName
                        );
                        break;

                    case ProjectType.Laravel:
                        CreateLaravelProject(
                            projectPath,
                            options.ProjectName
                        );
                        break;
                }

                OpenProject(
                    projectPath
                );

                return projectPath;
            }
            catch
            {
                return null;
            }
        }

        private static void CreateProjectFiles(
            string projectPath,
            string projectName,
            ProjectType projectType)
        {
            string assetsPath = Path.Combine(
                projectPath,
                "assets"
            );

            string stylePath = Path.Combine(
                assetsPath,
                "style"
            );

            string jsPath = Path.Combine(
                assetsPath,
                "js"
            );

            Directory.CreateDirectory(
                stylePath
            );

            Directory.CreateDirectory(
                jsPath
            );

            string cssPath = Path.Combine(
                stylePath,
                "style.css"
            );

            string javascriptPath = Path.Combine(
                jsPath,
                "script.js"
            );

            File.WriteAllText(
                cssPath,
                @"body {
margin: 0;
padding: 40px;
font-family: Arial, sans-serif;

}"
    );

            File.WriteAllText(
                javascriptPath,
                @"console.log(""WAMP-DS project ready."");"
            );

            switch (projectType)
            {
                case ProjectType.Html:
                    CreateHtmlProject(
                        projectPath,
                        projectName
                    );
                    break;

                case ProjectType.Php:

                case ProjectType.CodeIgniter:
                    CreatePhpProject(
                        projectPath,
                        projectName
                    );
                    break;

                case ProjectType.Laravel:
                    CreateLaravelProject(
                        projectPath,
                        projectName
                    );
                    break;
            }

            if (projectType == ProjectType.Laravel)
            {
                return;
            }
        }

        private static void CreateHtmlProject(
    string projectPath,
    string projectName)
        {
            string indexPath =
                Path.Combine(
                    projectPath,
                    "index.html"
                );


            File.WriteAllText(
                indexPath,
                $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">

    <title>{projectName}</title>

    <link rel=""stylesheet"" href=""assets/style/style.css"">
</head>

<body>

<h1>Welcome to {projectName}</h1>

<p>Your WAMP-DS project is ready.</p>

<script src=""assets/js/script.js""></script>

</body>
</html>"
            );
        }

        private static void CreatePhpProject(
    string projectPath,
    string projectName)
        {
            string indexPath =
                Path.Combine(
                    projectPath,
                    "index.php"
                );


            File.WriteAllText(
                indexPath,
                $@"<!DOCTYPE html>
<html lang=""en"">
<head>

<title>{projectName}</title>

<link rel=""stylesheet"" href=""assets/style/style.css"">

</head>

<body>

<h1>Welcome to {projectName}</h1>

<p>PHP is working.</p>

<?php
echo ""<p>PHP is working.</p>"";
?>

<script src=""assets/js/script.js""></script>

</body>
</html>"
            );
        }

        private static void CreateLaravelProject(
    string projectPath,
    string projectName)
        {
            File.WriteAllText(
                Path.Combine(
                    projectPath,
                    "README.txt"
                ),
                "Laravel project installation pending."
            );
        }

        public void CloseProject()
        {
            CurrentProjectPath = null;

            ProjectItems.Clear();
        }

        public void LoadProjectItems()
        {
            ProjectItems.Clear();

            if (string.IsNullOrEmpty(CurrentProjectPath))
                return;

            LoadDirectory(
                CurrentProjectPath,
                ProjectItems
            );
        }


        private static void LoadDirectory(
            string directoryPath,
            ObservableCollection<ProjectItem> items)
        {
            string[] directories;

            try
            {
                directories = Directory.GetDirectories(
                    directoryPath
                );
            }
            catch
            {
                return;
            }


            foreach (string directory in directories)
            {
                ProjectItem directoryItem = new()
                {
                    Name = Path.GetFileName(directory),
                    FullPath = directory,
                    IsDirectory = true,
                    IsLoaded = false
                };


                directoryItem.Children.Add(
                    new ProjectItem
                    {
                        Name = "Loading...",
                        IsPlaceholder = true
                    }
                );


                items.Add(directoryItem);
            }


            string[] files;

            try
            {
                files = Directory.GetFiles(
                    directoryPath
                );
            }
            catch
            {
                return;
            }


            foreach (string file in files)
            {
                items.Add(
                    new ProjectItem
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    }
                );
            }
        }



        public void LoadChildren(
    ProjectItem item)
        {
            if (!item.IsDirectory ||
                item.IsLoaded)
                return;


            item.Children.Clear();


            string[] directories;

            try
            {
                directories =
                    Directory.GetDirectories(
                        item.FullPath
                    );
            }
            catch
            {
                return;
            }


            foreach (string directory in directories)
            {
                ProjectItem directoryItem = new()
                {
                    Name = Path.GetFileName(directory),
                    FullPath = directory,
                    IsDirectory = true,
                    IsLoaded = false
                };

                directoryItem.Children.Add(
                    new ProjectItem
                    {
                        Name = "",
                        IsPlaceholder = true
                    }
                );

                item.Children.Add(
                    directoryItem
                );
            }


            string[] files;

            try
            {
                files =
                    Directory.GetFiles(
                        item.FullPath
                    );
            }
            catch
            {
                return;
            }


            foreach (string file in files)
            {
                item.Children.Add(
                    new ProjectItem
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    }
                );
            }


            item.IsLoaded = true;
        }
    }

    public class ProjectItem
    {
        public string Name { get; set; } = string.Empty;

        public string FullPath { get; set; } = string.Empty;

        public bool IsDirectory { get; set; }

        public ObservableCollection<ProjectItem> Children { get; } = new();

        public bool IsLoaded { get; set; }

        public bool IsPlaceholder { get; set; }
    }

}