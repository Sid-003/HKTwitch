using HollowTwitch.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

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
            Modding.Logger.Log("reached here execute");
            var pieces = command.Split(Seperator);
            var found = _commands.Where(x => x.Name.Equals(pieces[0], StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.Priority);
            Modding.Logger.Log("reached here execute past command finding");
            foreach (var c in found)
            {
                Modding.Logger.Log("reached loop");
                var args = pieces.Skip(1);
                var sucess = BuildArguments(args, c, out var parsed);
                if (!sucess)
                    continue;
                try
                {                 
                    c.MethodInfo.Invoke(c.ClassInstance, parsed);
                }
                catch (Exception e)
                {
                    Modding.Logger.Log(e.ToString());
                }
                Modding.Logger.Log("invoked");
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
            foreach (var method in methods)
            {
                if((method.GetCustomAttributes(typeof(HKCommandAttribute), false).FirstOrDefault() is HKCommandAttribute attribute))
                {
                    var name = attribute.Name;
                    _commands.Add(new Command(name, method, typeof(T)));
                    Modding.Logger.Log(name);
                    Modding.Logger.Log("added the command");
                }
            }
        }


    }
}
