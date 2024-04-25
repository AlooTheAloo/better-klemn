using System.Collections;
using System.Collections.Generic;
using Chroma.Editor;
using Lean.Pool;
using Nova;
using UnityEngine;

public class NoteInvoker
{
    private ICommand command;

    public static Stack<ICommand> _commandList;
    public static Stack<ICommand> _reverseHistory;

    public NoteInvoker()
    {
        _commandList = new Stack<ICommand>();
        _reverseHistory = new Stack<ICommand>();
    }

    public static void AddCommand(ICommand newCommand)
    {
        _reverseHistory.Clear();
        _commandList.Push(newCommand);
        _commandList.Peek().Execute();
    }
    public void Execute()
    {
        if (_reverseHistory.Count > 0)
        {
            ICommand command = _reverseHistory.Pop();
            _commandList.Push(command);
            command.Execute();
        }
    }

    public void Undo()
    {
        if (_commandList.Count > 0)
        {
            ICommand command = _commandList.Pop();
            _reverseHistory.Push(command);
            command.Undo();
        }
    }


}
