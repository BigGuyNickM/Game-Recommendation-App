using System;
using Game_Recommendation.Cli.Utils;

namespace Game_Recommendation.Cli.Managers
{
    public abstract class BaseMenu
    {
        protected bool isRunning = true;

        public void Run()
        {
            isRunning = true;
            while (isRunning)
            {
                _ShowMenu();
                string choice = InputHelper.GetInput();
                _HandleChoice(choice);
            }
        }

        protected abstract void _ShowMenu();
        protected abstract void _HandleChoice(string choice);

        protected void _Exit()
        {
            isRunning = false;
        }
    }
}