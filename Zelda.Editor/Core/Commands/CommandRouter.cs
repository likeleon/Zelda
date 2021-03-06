﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Core.Commands
{
    [Export(typeof(ICommandRouter))]
    public class CommandRouter : ICommandRouter
    {
        static readonly Type CommandHandlerInterfaceType = typeof(ICommandHandler<>);

        readonly Dictionary<Type, CommandHandlerWrapper> _globalCommandHandlerWrappers = new Dictionary<Type, CommandHandlerWrapper>();
        readonly Dictionary<Type, HashSet<Type>> _commandHandlerTypeToCommandDefinitionTypesLookup = new Dictionary<Type, HashSet<Type>>();

        [ImportingConstructor]
        public CommandRouter([ImportMany(typeof(ICommandHandler))] ICommandHandler[] globalCommandHandlers)
        {
            BuildCommandHandlerWrappers(globalCommandHandlers);
        }

        void BuildCommandHandlerWrappers(ICommandHandler[] commandHandlers)
        {
            foreach (var commandHandler in commandHandlers)
            {
                var commandHandlerType = commandHandler.GetType();
                EnsureCommandHandlerTypeToCommandDefinitionTypesPopulated(commandHandlerType);
                var commandDefinitionTypes = _commandHandlerTypeToCommandDefinitionTypesLookup[commandHandlerType];
                foreach (var commandDefinitionType in commandDefinitionTypes)
                    _globalCommandHandlerWrappers[commandDefinitionType] = CreateCommandHandlerWrapper(commandDefinitionType, commandHandler);
            }
        }

        void EnsureCommandHandlerTypeToCommandDefinitionTypesPopulated(Type commandHandlerType)
        {
            if (!_globalCommandHandlerWrappers.ContainsKey(commandHandlerType))
            {
                var commandDefinitionTypes = _commandHandlerTypeToCommandDefinitionTypesLookup[commandHandlerType] = new HashSet<Type>();

                foreach (var handledCommandDefinitionType in GetAllHandledCommandedDefinitionTypes(commandHandlerType, CommandHandlerInterfaceType))
                    commandDefinitionTypes.Add(handledCommandDefinitionType);
            }
        }

        static IEnumerable<Type> GetAllHandledCommandedDefinitionTypes(Type type, Type genericInterfaceType)
        {
            var result = new List<Type>();
            while (type != null)
            {
                result.AddRange(type.GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterfaceType)
                    .Select(x => x.GetGenericArguments().First()));
                
                type = type.BaseType;
            }

            return result;
        }

        static CommandHandlerWrapper CreateCommandHandlerWrapper(Type commandDefinitionType, object commandHandler)
        {
            if (typeof(CommandDefinition).IsAssignableFrom(commandDefinitionType))
                return CommandHandlerWrapper.FromCommandHandler(CommandHandlerInterfaceType.MakeGenericType(commandDefinitionType), commandHandler);
            if (typeof(CommandListDefinition).IsAssignableFrom(commandDefinitionType))
                return CommandHandlerWrapper.FromCommandListHandler(CommandHandlerInterfaceType.MakeGenericType(commandDefinitionType), commandHandler);
            throw new InvalidOperationException();
        }

        public CommandHandlerWrapper GetCommandHandler(CommandDefinitionBase commandDefinition)
        {
            CommandHandlerWrapper commandHandler;

            var activeItemViewModel = IoC.Get<IShell>().ActiveLayoutItem;
            if (activeItemViewModel != null)
            {
                var activeItemView = ViewLocator.LocateForModel(activeItemViewModel, null, null);
                var activeItemWindow = Window.GetWindow(activeItemView);
                if (activeItemWindow != null)
                {
                    var startElement = FocusManager.GetFocusedElement(activeItemWindow);

                    commandHandler = FindCommandHandlerInVisualTree(commandDefinition, startElement);
                    if (commandHandler != null)
                        return commandHandler;
                }
            }

            if (!_globalCommandHandlerWrappers.TryGetValue(commandDefinition.GetType(), out commandHandler))
                return null;

            return commandHandler;
        }

        CommandHandlerWrapper FindCommandHandlerInVisualTree(CommandDefinitionBase commandDefinition, IInputElement target)
        {
            var visualObject = target as DependencyObject;
            if (visualObject == null)
                return null;

            object previousDataContext = null;
            do
            {
                var frameworkElement = visualObject as FrameworkElement;
                if (frameworkElement != null)
                {
                    var dataContext = frameworkElement.DataContext;
                    if (dataContext != null && !ReferenceEquals(dataContext, previousDataContext))
                    {
                        if (dataContext is ICommandRerouter)
                        {
                            var commandRerouter = (ICommandRerouter)dataContext;
                            var commandTarget = commandRerouter.GetHandler(commandDefinition);
                            if (commandTarget != null)
                            {
                                if (IsCommandHandlerForCommandDefinitionType(commandTarget, commandDefinition.GetType()))
                                    return CreateCommandHandlerWrapper(commandDefinition.GetType(), commandTarget);
                                throw new InvalidOperationException("This object does not handle the specified command definition.");
                            }
                        }

                        if (IsCommandHandlerForCommandDefinitionType(dataContext, commandDefinition.GetType()))
                            return CreateCommandHandlerWrapper(commandDefinition.GetType(), dataContext);

                        previousDataContext = dataContext;
                    }
                }
                visualObject = VisualTreeHelper.GetParent(visualObject);
            } while (visualObject != null);

            return null;
        }

        bool IsCommandHandlerForCommandDefinitionType(object commandHandler, Type commandDefinitionType)
        {
            var commandHandlerType = commandHandler.GetType();
            EnsureCommandHandlerTypeToCommandDefinitionTypesPopulated(commandHandlerType);
            var commandDefinitionTypes = _commandHandlerTypeToCommandDefinitionTypesLookup[commandHandlerType];
            return commandDefinitionTypes.Contains(commandDefinitionType);
        }
    }
}
