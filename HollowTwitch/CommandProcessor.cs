using HollowTwitch.Entities;
using HollowTwitch.Precondition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace HollowTwitch
{
    //scuffed command proccersor thingy, needs a lot of work
    public class CommandProcessor
    {
        private const char Seperator = ' ';

        private readonly List<Command> _commands;
        private readonly Dictionary<Type, IArgumentParser> _parsers;

        public CommandProcessor()
        {
            _commands = new List<Command>();
            _parsers = new Dictionary<Type, IArgumentParser>();
        }

        public void Execute(string command)
        {
            var pieces = command.Split(Seperator);
            var found = _commands.Where(x => x.Name.Equals(pieces[0], StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.Priority);
            foreach (var c in found)
            {
                /*if (!c.Preconditions.All(x => x.Check()))
                    continue;*/
                bool allGood = true;
                foreach(var p in c.Preconditions)
                {
                    if(p.Check() != true)
                    {
                        allGood = false;
                        if(c.Preconditions.FirstOrDefault() is CooldownAttribute cooldown)
                            Modding.Logger.Log($"The coodown for command {c.Name} failed. The cooldown has {cooldown.MaxUses - cooldown.Uses} and will reset in {cooldown.ResetTime - DateTimeOffset.Now}");       
                    }
                }
                if (!allGood)
                    continue;

                var args = pieces.Skip(1);
                if (!BuildArguments(args, c, out var parsed))
                    continue;
                try
                {
                    Modding.Logger.Log("got here");
                    if(c.MethodInfo.ReturnType == typeof(IEnumerator))
                    {
                        var t = c.MethodInfo.Invoke(c.ClassInstance, parsed) as IEnumerator;
                        GameManager.instance.StartCoroutine(t);
                    }
                    else
                    {
                        _ = c.MethodInfo.Invoke(c.ClassInstance, parsed);
                    }
                }
                catch (Exception e)
                {
                    Modding.Logger.Log(e.ToString());
                }
            }
                                 

        }

        public bool BuildArguments(IEnumerable<string> args, Command command, out object[] parsed) 
        {
            parsed = null;
            var parameters = command.Parameters;
            var built = new List<object>();
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = ParseParameter(args.ElementAt(i), parameters[i].ParameterType);
                if (p is null)
                    return false;
                built.Add(p);
            }
            parsed = built.ToArray();
            return true;
        }

        public object ParseParameter(string arg, Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);
            try
            {
                return converter.ConvertFromString(arg);
            }
            catch
            {
                try
                {
                    var parsed = _parsers[type].Parse(arg);
                    return parsed;
                }
                catch 
                {
                    return null;
                }
            }
        }

        public void RegisterCommands<T>()
        {
            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var instance = Activator.CreateInstance(typeof(T));
            foreach (var method in methods)
            {
                if((method.GetCustomAttributes(typeof(HKCommandAttribute), false).FirstOrDefault() is HKCommandAttribute attribute))
                {
                    var name = attribute.Name;
                    _commands.Add(new Command(name, method, instance));
                    Modding.Logger.Log(name);
                    Modding.Logger.Log("added the command");
                }
            }
        }


    }
}
