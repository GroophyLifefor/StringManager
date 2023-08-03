using System.Text;

namespace Clee.Text;

public class StringManager : IDisposable
{
    private bool _disposed;
    private string _text;
    private int _currentIndex = 0;

    public string Text => _text;
    public delegate void OnLogDelegate(string log);
    public event OnLogDelegate? OnLog;

    /// <summary>
    /// Initializes a new instance of the StringManager class with the given text.
    /// </summary>
    /// <param name="text">The text to be managed by the StringManager.</param>
    public StringManager(string text)
        => _text = text ?? throw new ArgumentNullException(nameof(text));
    
    ~StringManager()
    {
        this.Dispose(false);
    }

    private void InvokeLogEvent(string log)
        => OnLog?.Invoke(log);
    
    /// <summary>
    /// Gets the substring before the specified key starting from the given startIndex.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <param name="startIndex">The index to start the search from. Default is 0.</param>
    /// <returns>The substring before the key or null if the key is not found.</returns>
    public string? GetBefore(string key, int startIndex = 0)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        string tempAfter = startIndex == 0 ? _text : _text.Substring(startIndex);
        int nextKey = tempAfter.IndexOf(key, StringComparison.Ordinal);

        if (nextKey == -1) return null;

        return tempAfter.Substring(0, nextKey);
    }
    
    /// <summary>
    /// Gets the substring after the specified key starting from the given startIndex.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <param name="startIndex">The index to start the search from. Default is 0.</param>
    /// <returns>The substring after the key or null if the key is not found.</returns>
    public string? GetAfter(string key, int startIndex = 0)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        string tempAfter = startIndex == 0 ? _text : _text.Substring(startIndex);
        int nextKey = tempAfter.IndexOf(key, StringComparison.Ordinal);

        if (nextKey == -1) return null;

        return tempAfter.Substring(nextKey + key.Length);
    }
    
    /// <summary>
    /// Gets the substring after the specified index.
    /// </summary>
    /// <param name="index">The index to start the substring from.</param>
    /// <returns>The substring after the specified index or null if the index is out of range.</returns>
    public string? GetAfter(int index) 
        => _text.Length > index ? null : _text.Substring(index);
    
    /// <summary>
    /// Gets the substring after the specified index with the given length.
    /// </summary>
    /// <param name="index">The index to start the substring from.</param>
    /// <param name="length">The length of the substring to get.</param>
    /// <returns>The substring with the given length after the specified index or null if the index is out of range.</returns>
    public string? GetAfter(int index, int length) 
        => _text.Length > index + length ? null : _text.Substring(index, length);

    /// <summary>
    /// Replaces a portion of the managed text with the provided append string.
    /// </summary>
    /// <param name="index">The index to start the replacement from. Default is 0.</param>
    /// <param name="removeLength">The length of the substring to remove. Default is 0.</param>
    /// <param name="append">The string to append at the specified index. Default is an empty string.</param>   
    public void Replace(int index = 0, int removeLength = 0, string append = "")
    {
        _text = _text.Remove(index, removeLength);
        _text = _text.Insert(index, append);

        if (_currentIndex != 0 && _currentIndex > index)
        {
            _currentIndex += append.Length - removeLength;
        }
    }

    /// <summary>
    /// Exports an array of Wildcard objects from the given pattern.
    /// </summary>
    /// <param name="pattern">The pattern containing wildcards to export.</param>
    /// <returns>An array of Wildcard objects parsed from the pattern.</returns>
    public static Wildcard[] ExportWildcards(string pattern)
    {
        List<Wildcard> patternList = new List<Wildcard>();

        bool isNamed = false;
        StringBuilder tempName = new StringBuilder();
        for (int i = 0; i < pattern.Length; i++)
        {
            char _c = pattern[i];

            if (_c == '*')
            {
                if (tempName.Length != 0)
                {
                    patternList.Add(new ()
                    {
                        IsStatic = true,
                        Name = null,
                        Value = tempName.ToString()
                    });
                    tempName.Clear();
                }

                patternList.Add(new ()
                {
                    IsStatic = false,
                    Name = null,
                    Value = null
                });
                continue;
            }

            if (_c == '[' && !isNamed)
            {
                isNamed = true;
                if (tempName.Length != 0)
                {
                    patternList.Add(new ()
                    {
                        IsStatic = true,
                        Name = null,
                        Value = tempName.ToString()
                    });
                    tempName.Clear();
                }

                continue;
            }

            if (_c == ']' && isNamed)
            {
                isNamed = false;
                patternList.Add(new ()
                {
                    IsStatic = false,
                    Name = tempName.ToString(),
                    Value = null
                });
                tempName.Clear();
                continue;
            }

            tempName.Append(_c);
        }

        if (tempName.Length != 0)
        {
            patternList.Add(new ()
            {
                IsStatic = true,
                Name = null,
                Value = tempName.ToString()
            });
            tempName.Clear();
        }


        return patternList.ToArray();
    }

    /// <summary>
    /// Applies the provided wildcards to the managed text using the given delegate for modification or replacement.
    /// </summary>
    /// <param name="wildcards">The array of Wildcard objects to apply.</param>
    /// <param name="_delegate">The delegate to perform the modification or replacement on the text.</param>
    /// <param name="caseSensitive">Determines if the wildcards are case-sensitive. Default is false.</param>
    /// <param name="wildcardName">The name of the wildcard for logging purposes. Default is an empty string.</param>
    public void ApplyWildcards(
        Wildcard[] wildcards, 
        Action<ModifyWildcard> _delegate, 
        bool caseSensitive = false,
        string wildcardName = "")
    {
        if (wildcards.Length == 0) return;
        
        if (wildcards[0].Value != null)
        {
            _currentIndex = _text.IndexOf(wildcards[0].Value!,
                caseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            
            if (_currentIndex == -1) return;
        }

        var wildcardIndex = 0;
        var modify = new ModifyWildcard(this) { StartIndex = _currentIndex};
        var isContinue = true;

        string GetAfterText()
        {
            if (_currentIndex >= 0 && _currentIndex <= _text.Length) return _text[_currentIndex..];
            return string.Empty;
        }

        while (isContinue)
        {

            if (wildcards.Length == wildcardIndex)
            {
                modify.EndIndex = _currentIndex;
                _delegate.Invoke(modify);
                var afterText = GetAfterText();

                int _ = caseSensitive
                    ? afterText.IndexOf(wildcards[0].Value!, StringComparison.InvariantCultureIgnoreCase)
                    : afterText.IndexOf(wildcards[0].Value!, StringComparison.InvariantCulture);
                _currentIndex = _ == -1 ? _currentIndex : _ + _currentIndex;
                modify = new (this) { StartIndex = _currentIndex };
                wildcardIndex = 0;
            }

            if (wildcards[wildcardIndex].IsStatic)
            {
                int newIndex = GetAfterText().IndexOf(wildcards[wildcardIndex].Value!,
                    caseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                if (newIndex != -1)
                {
                    // expected
                    if (wildcardIndex == 0) modify.StartIndex = _currentIndex + newIndex;

                    _currentIndex += newIndex + wildcards[wildcardIndex].Value!.Length;
                    InvokeLogEvent($"{(!string.IsNullOrEmpty(wildcardName) ? $"'{wildcardName}' " : "")}from Wildcard[{wildcardIndex}/{(wildcards.Length - 1)}] found at index {_currentIndex} IsStatic: {wildcards[wildcardIndex].IsStatic}, Value: \"{FixSlashes(wildcards[wildcardIndex].Value)}\"");
                    wildcardIndex++;
                }
                else
                {
                    // not found anymore
                    InvokeLogEvent($"{(!string.IsNullOrEmpty(wildcardName) ? $"'{wildcardName}' " : "")}from Wildcard[{wildcardIndex}/{(wildcards.Length - 1)}] --NOT-- found IsStatic: {wildcards[wildcardIndex].IsStatic}, Value: \"{FixSlashes(wildcards[wildcardIndex].Value)}\"");
                    isContinue = false;
                    wildcardIndex = 0;
                }
            }
            else
            {
                var _next = wildcards.Length > wildcardIndex + 1 ? wildcards[wildcardIndex + 1] : null;

                int newIndex = _next is null
                    ? GetAfter(_currentIndex).Length
                    : GetAfterText().IndexOf(_next.Value,
                        caseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

                if (newIndex == -1)
                {
                    wildcardIndex = 0;
                    InvokeLogEvent($"{(!string.IsNullOrEmpty(wildcardName) ? $"'{wildcardName}' " : "")}from Wildcard[{wildcardIndex}/{(wildcards.Length - 1)}] --NOT-- found at index {_currentIndex} IsStatic: {wildcards[wildcardIndex].IsStatic}, Name: {wildcards[wildcardIndex].Name}, Value: \"{FixSlashes(wildcards[wildcardIndex].Value)}\"");
                    continue;
                }

                string Value = GetAfterText().Substring(0, newIndex);

                if (wildcards[wildcardIndex].Name is null)
                {
                    if (!ContainsOnlySpacesAndTabs(Value))
                    {
                        wildcardIndex = 0;
                        InvokeLogEvent($"{(!string.IsNullOrEmpty(wildcardName) ? $"'{wildcardName}' " : "")}from Wildcard[{wildcardIndex}/{(wildcards.Length - 1)}] --NOT-- found because special wildcard has non SpacesAndTabs character."); 
                        continue;
                    }
                }
                else
                {
                    modify.WildcardIndexes.Add(new ()
                    {
                        Name = wildcards[wildcardIndex].Name,
                        Value = Value,
                        Index = _currentIndex
                    });
                    InvokeLogEvent($"{(!string.IsNullOrEmpty(wildcardName) ? $"'{wildcardName}' " : "")}from Wildcard[{wildcardIndex}/{(wildcards.Length - 1)}] found at index {_currentIndex} IsStatic: {wildcards[wildcardIndex].IsStatic}, Name: \"{wildcards[wildcardIndex].Name}\"");
                }

                _currentIndex += Value.Length;
                wildcardIndex++;
            }
        }

        _currentIndex = 0;
    }

    private string FixSlashes(string value)
        => value is null ? string.Empty : value.Replace("\r\n", "\\r\\n").Replace("\n", "\\n").Replace("\t", "\\t");

    private bool ContainsOnlySpacesAndTabs(string input)
    {
        foreach (char ch in input)
        {
            int num;
            switch (ch)
            {
                case '\t':
                case '\n':
                case ' ':
                    num = 0;
                    break;
                default:
                    num = ch != '\r' ? 1 : 0;
                    break;
            }
            if (num != 0)
                return false;
        }
        return true;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) 
            {
                // Dispose managed resources here.
                // TODO
            }

            // Dispose unmanaged resources here.
            // TODO
        }

        _disposed = true;
    }

    
}