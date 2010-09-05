using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Shell;

namespace IMAPShell.Interfaces
{
    public interface ICommand
    {
        CommandResult Execute();
    }
}
