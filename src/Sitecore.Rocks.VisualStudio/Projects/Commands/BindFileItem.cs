// � 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Projects.Dialogs;
using Sitecore.Rocks.Projects.FileItems;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.BindFileItem)]
    public class BindFileItem : SolutionCommand
    {
        public override bool CanExecute(object parameter)
        {
            IsVisible = false;

            if (!(parameter is ShellContext))
            {
                return false;
            }

            var selectedItems = GetSelectedItems();
            if (selectedItems.Count != 1)
            {
                return false;
            }

            var items = GetProjectItems(selectedItems);
            if (items.Count != 1)
            {
                return false;
            }

            if (!items.All(i => i.ContainingProject.IsValidProjectKind()))
            {
                return false;
            }

            if (AnyItem(items, item => ProjectManager.GetProject(item) == null))
            {
                return false;
            }

            IsVisible = AnyItem(items, IsFileItem);

            return IsVisible;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                return;
            }

            var items = GetProjectItems(selectedItems);

            var project = ProjectManager.GetProject(items[0]);
            if (project == null)
            {
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                return;
            }

            var dialog = new BindFileItemDialog();
            dialog.Initialize(site, items);
            AppHost.Shell.ShowDialog(dialog);
        }

        private bool IsFileItem([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return false;
            }

            var projectFile = project.GetProjectItem(item) as ProjectFileItem;
            if (projectFile != null)
            {
                var itemIds = projectFile.Items;
                if (itemIds.Count > 0)
                {
                    return false;
                }
            }

            var fileName = item.GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return FileItemManager.GetFileItemHandler(fileName) != null;
        }
    }
}
