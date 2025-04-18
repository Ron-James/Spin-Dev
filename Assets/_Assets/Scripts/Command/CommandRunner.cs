using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static System.Activator;


public interface ICommand
{
    Task Execute();
    public bool IsExecuting { get; }
    
}

[ShowOdinSerializedPropertiesInInspector]
[Serializable]
public abstract class BaseCommand : ICommand
{
    [SerializeField, ReadOnly] protected string Name;
    [SerializeField, ReadOnly] protected bool isExecuting;
    public abstract Task Execute();
    
    
    public BaseCommand()
    {
        Name = GetType().Name;
    }
    
    public BaseCommand(string name)
    {
        Name = name;
    }
    public bool IsExecuting
    {
        get => isExecuting;
        protected set => value = IsExecuting;
    }
    
}
[Serializable]
public class DelayedActionCmd : BaseCommand
{
    private UnityAction _action;
    private float _delay;
    public DelayedActionCmd()
    {
        
    }
    public DelayedActionCmd(UnityAction action, float delay)
    {
        _action = action;
        _delay = delay;
        Name = GetType().Name;
    }
    public DelayedActionCmd(string name, UnityAction action, float delay) : base(name)
    {
        _action = action;
        _delay = delay;
    }
    public override async Task Execute()
    {
        IsExecuting = true;
        await Awaitable.WaitForSecondsAsync(_delay);
        _action();
        IsExecuting = false;
    }
}
[Serializable]
public class AsyncCommand : BaseCommand
{
    private Func<Task> _action;

    public AsyncCommand(Func<Task> action)
    {
        _action = action;
    }
    
    public AsyncCommand(string name, Func<Task> action) : base(name)
    {
        _action = action;
    }

    public override async Task Execute()
    {
        IsExecuting = true;
        await _action();
        IsExecuting = false;
    }
}


[ExecuteAlways]
public class CommandRunner : PersistentMonoBehaviour<CommandRunner>
{
    
    private static List<ICommand> ongoingCommands = new();
    private static List<ICommand> _completedCommands = new();
    
    
    private static CommandInvoker _invoker = new CommandInvoker();
    
    [Title("Commands")]
    [ShowInInspector]
    public List<ICommand> OnGoingCommands => ongoingCommands;
    
    [ShowInInspector]
    public List<ICommand> CompletedCommands => _completedCommands;
    
    

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        _completedCommands = new List<ICommand>();
        ongoingCommands = new List<ICommand>();
        Initialize();
    }
    private void OnEnable()
    {
        _invoker = new CommandInvoker();
        _invoker.OnCommandExecuted += OnCommandExecuted;
    }
    
    private void OnDisable()
    {
        _invoker.OnCommandExecuted -= OnCommandExecuted;
        _invoker = null;
    }
    
    
    private void OnCommandExecuted(ICommand command)
    {
        if (ongoingCommands.Contains(command))
        {
            ongoingCommands.Remove(command);
            _completedCommands.Add(command);
        }
    }


    public static async void ExecuteSimultaneously(List<ICommand> commands)
    {
        ongoingCommands.AddRange(commands);
        await _invoker.InvokeSimultaneously(commands);
    }


    public static async void ExecuteCommand(ICommand command)
    {
        ongoingCommands.Add(command);
        await _invoker.ExecuteCommand(command);
        
    }


    [Serializable]
    private class CommandInvoker
    {
        public event Action<ICommand> OnCommandStarted;
        public event Action<ICommand> OnCommandExecuted;
        public async Task ExecuteCommand(ICommand command)
        {
            try
            {
                OnCommandStarted?.Invoke(command);
                await command.Execute();
                OnCommandExecuted?.Invoke(command);
            }
            catch (OperationCanceledException e)
            {
#if UNITY_EDITOR
                    Debug.Log(e.Message);
#endif
            }
        }

        public async Task InvokeSimultaneously(List<ICommand> commands)
        {
            var tasks = new List<Task>();
            foreach (var command in commands)
            {
                tasks.Add(ExecuteCommand(command));
            }

            await Task.WhenAll(tasks);
        }

        public async Task InvokeSequentially(List<ICommand> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                await ExecuteCommand(commands[i]);
                
            }
        }

        public async Task InvokeReverse(List<ICommand> commands)
        {
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                try
                {
                    await ExecuteCommand(commands[i]);
                }
                catch (OperationCanceledException e)
                {
#if UNITY_EDITOR
                        Debug.Log(e.Message);
#endif
                }
            }
        }
    }
}