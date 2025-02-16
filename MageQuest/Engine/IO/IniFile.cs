// modified from original
// source: https://stackoverflow.com/a/14906422

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace MageQuest.IO;

public class IniFile(string data = null)
{
    readonly IniRoot _parsed = Parse(data);

    public IniRoot Data => _parsed;

    public string Get(string Key, string Section, string Default = null)
    {
        return _parsed.GetValueOrDefault(Key, Default, Section);
    }

    public void Set(string Key, string Value, string Section)
    {
        _parsed[Section] ??= [];
        _parsed[Section][Key] = Value;
    }

    public void DeleteKey(string Key, string Section)
    {
        _parsed[Section]?.Remove(Key);
    }

    public void DeleteSection(string Section)
    {
        _parsed.Remove(Section);
    }

    public bool KeyExists(string Key, string Section)
    {
        return Get(Key, Section) is not null;
    }

    public void Write(Stream stream)
    {
        foreach(var section in _parsed)
        {
            byte[] sectionTitle = Encoding.UTF8.GetBytes($"[{section.Key}]\n");
            stream.Write(sectionTitle, 0, sectionTitle.Length);

            foreach(var key in section.Value)
            {
                byte[] value = Encoding.UTF8.GetBytes($"{key.Key}={key.Value}\n");
                stream.Write(value, 0, value.Length);
            }

            stream.WriteByte(Encoding.UTF8.GetBytes((char[])['\n'])[0]);
        }
    }

    public void Write(string path)
    {
        using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write);
        Write(fileStream);
    }

    public static IniRoot Parse(string data, bool verboseOutput = false)
    {
        using StringReader reader = new(data);
        IniRoot result = [];

        IniDict<string> currentSection = null;
        string currentSectionName = null;

        int lineNum = 0;
        while(reader.Peek() != -1)
        {
            var line = reader.ReadLine();
            lineNum++;

            // ignore empty lines
            if(line is null || line == "")
            {
                if(verboseOutput)
                    Console.WriteLine($"{lineNum}: empty");
                continue;
            }

            // fix windows format
            if(line[^1] == '\r')
            {
                line = line[..^1];
                if(verboseOutput)
                    Console.WriteLine($"{lineNum}: CR");
                if(line == "") continue;
            }

            // // multiline
            // while(line[^1] == '\\')
            // {
            //     var old = line[..^1] + '\n';
            //     line = $"{old}{reader.ReadLine()}";

            //     if(line.Length > old.Length && line[old.Length] == ';')
            //     {
            //         line = $"{old}{reader.ReadLine()}";
            //     }
            // }
            // if(line == null) continue;

            // ignore preceeding whitespace
            while(line[0] == ' ' || line[0] == '\t')
            {
                line = line[1..];
                if(verboseOutput)
                    Console.WriteLine($"{lineNum}: preceeding whitespace");
            }
            if(line == "" || line[0] == ';')
            {
                if(verboseOutput)
                    Console.WriteLine($"{lineNum}: all whitespace, or comment");
                continue;
            }

            // read section
            if(line[0] == '[')
            {
                if(line[^1] != ']') continue;

                if(currentSection is not null)
                {
                    result[currentSectionName] = currentSection;
                    if(verboseOutput)
                        Console.WriteLine($"parsed section '{currentSectionName}': {string.Join(", ", currentSection)}");
                }

                currentSectionName = line[1..^1];
                currentSection = [];

                if(verboseOutput)
                    Console.WriteLine($"{lineNum}: begin section '{currentSectionName}'");
                continue;
            }

            // read value
            int ind = line.IndexOf('=');
            if(ind <= 0) continue;

            string key = line[..ind];
            string value = line[(ind + 1)..];

            if(verboseOutput)
                Console.WriteLine($"{lineNum}: value '{key}={value}'");

            currentSection[key] = value;
        }

        if(currentSection is not null)
        {
            result[currentSectionName] = currentSection;
            if(verboseOutput)
                Console.WriteLine($"parsed section '{currentSectionName}': {string.Join(", ", currentSection)}");
        }

        return result;
    }

    public class IniRoot : IniDict<IniDict<string>>
    {
        public string GetValueOrDefault(string key, string defaultValue, string sectionName)
        {
            if(!this.ContainsKey(sectionName)) return defaultValue;

            return this[sectionName].GetValueOrDefault(key, defaultValue);
        }
    }

    public class IniDict<T> : IDictionary<string, T>
    {
        private readonly IDictionary<string, T> _internal = new Dictionary<string, T>();

        public virtual T GetDefaultValue() => default;

        public T this[string key]
        {
            get => GetValueOrDefault(key, GetDefaultValue());
            set => _internal[key] = value;
        }

        public ICollection<string> Keys => _internal.Keys;

        public ICollection<T> Values => _internal.Values;

        public int Count => _internal.Count;

        public bool IsReadOnly => _internal.IsReadOnly;

        public void Add(string key, T value)
        {
            _internal.Add(key, value);
        }

        public void Add(KeyValuePair<string, T> item)
        {
            _internal.Add(item.Key, item.Value);
        }

        public T GetValueOrDefault(string key, T defaultValue)
        {
            return _internal.TryGetValue(key, out T value) ? value : defaultValue;
        }

        public void Clear()
        {
            _internal.Clear();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return _internal.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _internal.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            _internal.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return _internal.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _internal.Remove(key);
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return _internal.Remove(item);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
        {
            return _internal.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
