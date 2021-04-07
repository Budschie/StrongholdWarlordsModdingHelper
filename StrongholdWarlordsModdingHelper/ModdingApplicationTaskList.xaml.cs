using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

namespace StrongholdWarlordsModdingHelper
{
    /// <summary>
    /// Interaction logic for ModdingApplicationTaskList.xaml
    /// </summary>
    public partial class ModdingApplicationTaskList : Window
    {
        ModdingProgressTask[] tasks;

        Stopwatch stopwatchTask = new Stopwatch();
        Stopwatch stopwatchAllTasks = new Stopwatch();

        public ModdingApplicationTaskList(params ModdingProgressTask[] tasks)
        {
            InitializeComponent();

            this.tasks = tasks;
            this.progressBar.Maximum = tasks.Length;
        }

        public void FinishTask(int taskIndex)
        {
            this.taskText.Text += "\nFinished task " + tasks[taskIndex].TaskName + " after " + stopwatchTask.ElapsedMilliseconds + " milliseconds.";
            this.progressBar.Value = taskIndex + 1;
        }

        public void StartTask(int taskIndex)
        {
            stopwatchTask.Restart();
            this.taskText.Text += "\nStarted task " + tasks[taskIndex].TaskName + "...";
            this.taskTitle.Text = "Task " + (taskIndex + 1).ToString() + " of " + tasks.Length.ToString() + ": " + tasks[taskIndex].TaskName;

            if (taskIndex == 0)
            {
                stopwatchAllTasks.Restart();
            }
        }

        public void SetDone()
        {
            this.taskTitle.Text = "All tasks done after " + stopwatchAllTasks.ElapsedMilliseconds + " milliseconds";
            this.taskText.Text += "\nDone!";
            this.progressBar.Maximum = 0;
        }
    }

    public class ModdingProgressTask
    {
        public string TaskName
        {
            get => taskName;
        }

        private string taskName;

        public ModdingProgressTask(string taskName)
        {
            this.taskName = taskName;
        }
    }
}
