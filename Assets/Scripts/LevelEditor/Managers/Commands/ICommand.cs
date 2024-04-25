using System.Collections;
using System.Collections.Generic;
using Chroma.Editor;
using Lean.Pool;
using Nova;
using UnityEngine;

public interface ICommand
{
    void Execute();

    void Undo();
}

