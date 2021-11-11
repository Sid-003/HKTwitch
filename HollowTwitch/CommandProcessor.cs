using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace HollowTwitch
{
    // Scuffed command proccersor thingy, needs a lot of work
    public class CommandProcessor
    {
        private const char Seperator = ' ';

        internal List<Command> Commands { get; }
        
        private readonly Dictionary<Type, IArgumentParser> _parsers;
        
        private readonly MonoBehaviour _coroutineRunner;

        public CommandProcessor()
        {
            Commands = new List<Command>();
            _parsers = new Dictionary<Type, IArgumentParser>();

            var go = new GameObject();

            UObject.DontDestroyOnLoad(go);

            _coroutineRunner = go.AddComponent<NonBouncer>();
        }

        public void AddTypeParser<T>(T parser, Type t) where T : IArgumentParser
        {
            _parsers.Add(t, parser);
        }

        public void Execute(string user, string command, ReadOnlyCollection<string> blacklist, bool ignoreChecks = false)
        {
            string[] pieces = command.Split(Seperator);

            IOrderedEnumerable<Command> found = Commands
                                                .Where(x => x.Name.Equals(pieces[0], StringComparison.InvariantCultureIgnoreCase))
                                                .OrderByDescending(x => x.Priority);
            
            foreach (Command c in found)
            {
                bool allGood = !blacklist.Contains(c.Name, StringComparer.OrdinalIgnoreCase);

                foreach (PreconditionAttribute p in c.Preconditions)
                {
                    if (p.Check(user)) continue;

                    allGood = false;

                    if (c.Preconditions.FirstOrDefault() is CooldownAttribute cooldown)
                    {
                        Logger.Log
                        (
                            $"The coodown for command {c.Name} failed. "
                            + $"The cooldown has {cooldown.MaxUses - cooldown.Uses} and will reset in {cooldown.ResetTime - DateTimeOffset.Now}"
                        );
                    }
                }

                allGood |= ignoreChecks;

                if (!allGood)
                    continue;

                IEnumerable<string> args = pieces.Skip(1);

                if (!BuildArguments(args, c, out object[] parsed))
                    continue;
                
                foreach (PreconditionAttribute precond in c.Preconditions)
                {
                    precond.Use();
                }

                try
                {
                    Logger.Log($"Built arguments for command {command}.");

                    IEnumerator RunCommand()
                    {
                        /*
                         * We have to wait a frame in order to make Unity itself call
                         * the MoveNext on the IEnumerator
                         *
                         * This forces it to run on the main thread, so Unity doesn't break.
                         */
                        yield return null;
                        
                        if (c.MethodInfo.ReturnType == typeof(IEnumerator))
                        {
                            yield return c.MethodInfo.Invoke(c.ClassInstance, parsed) as IEnumerator;
                        }
                        else
                        {
                            c.MethodInfo.Invoke(c.ClassInstance, parsed);
                        }
                    }

                    _coroutineRunner.StartCoroutine(RunCommand());

                }
                catch (Exception e)
                {
                    Logger.Log(e.ToString());
                }
            }
        }

        private bool BuildArguments(IEnumerable<string> args, Command command, out object[] parsed)
        {
            parsed = null;

            // Avoid multiple enumerations when indexing
            string[] enumerated = args.ToArray();
            
            ParameterInfo[] parameters = command.Parameters;

            bool hasRemainder = parameters.Length != 0 && parameters[parameters.Length - 1].GetCustomAttributes(typeof(RemainingTextAttribute), false).Any();
            
            if (enumerated.Length < parameters.Length && !hasRemainder)
                return false;
            
            var built = new List<object>();

            for (int i = 0; i < parameters.Length; i++)
            {
                string toParse = enumerated[i];
                if (i == parameters.Length - 1)
                {
                    if (hasRemainder)
                    {
                        toParse = string.Join(Seperator.ToString(), enumerated.Skip(i).Take(enumerated.Length).ToArray());
                    }
                }
                
                object p = ParseParameter(toParse, parameters[i].ParameterType);

                if (p is null)
                    return false;

                if (parameters[i].GetCustomAttributes(typeof(EnsureParameterAttribute), false).FirstOrDefault() is EnsureParameterAttribute epa)
                    p = epa.Ensure(p);

                built.Add(p);
            }

            parsed = built.ToArray();

            return true;
        }

        private object ParseParameter(string arg, Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);

            try
            {
                return converter.ConvertFromString(arg);
            }
            catch
            {
                try
                {
                    return _parsers[type].Parse(arg);
                }
                catch
                {
                    return null;
                }
            }
        }

        public void RegisterCommands<T>()
        {
            MethodInfo[] methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance);

            object instance = Activator.CreateInstance(typeof(T));

            foreach (MethodInfo method in methods)
            {
                HKCommandAttribute attr = method.GetCustomAttributes(typeof(HKCommandAttribute), false).OfType<HKCommandAttribute>().FirstOrDefault();

                if (attr == null)
                    continue;

                Commands.Add(new Command(attr.Name, method, instance));
                
                Logger.Log($"Added command: {attr.Name}");
            }
        }
    }
}