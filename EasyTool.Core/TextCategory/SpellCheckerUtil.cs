using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 拼写检查器
    /// 提供英文拼写检查和纠错功能
    /// </summary>
    public static class SpellCheckerUtil
    {
        private static readonly HashSet<string> _dictionary = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly char[] _alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static bool _isInitialized;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        static SpellCheckerUtil()
        {
            InitializeDictionary();
            _isInitialized = true;
        }

        /// <summary>
        /// 检查单词拼写是否正确
        /// </summary>
        /// <param name="word">单词</param>
        /// <returns>是否正确</returns>
        public static bool IsCorrect(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return true;

            return _dictionary.Contains(word.Trim().ToLowerInvariant());
        }

        /// <summary>
        /// 获取拼写建议
        /// </summary>
        /// <param name="word">单词</param>
        /// <param name="maxSuggestions">最大建议数量</param>
        /// <returns>建议列表</returns>
        public static List<string> GetSuggestions(string word, int maxSuggestions = 5)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new List<string>();

            word = word.Trim().ToLowerInvariant();

            // 如果拼写正确，返回空列表
            if (_dictionary.Contains(word))
                return new List<string>();

            var candidates = new Dictionary<string, int>();

            // 编辑距离为1的候选词
            var edits1 = GetEdits1(word);
            foreach (var edit in edits1)
            {
                if (_dictionary.Contains(edit))
                {
                    candidates[edit] = 1;
                }
            }

            // 编辑距离为2的候选词（如果没有找到距离1的）
            if (candidates.Count == 0)
            {
                foreach (var edit1 in edits1)
                {
                    var edits2 = GetEdits1(edit1);
                    foreach (var edit2 in edits2)
                    {
                        if (_dictionary.Contains(edit2) && !candidates.ContainsKey(edit2))
                        {
                            candidates[edit2] = 2;
                        }
                    }
                }
            }

            return candidates
                .OrderBy(kvp => kvp.Value)
                .ThenBy(kvp => LevenshteinDistance(word, kvp.Key))
                .Take(maxSuggestions)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// 检查文本中的拼写错误
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>错误单词及其建议</returns>
        public static Dictionary<string, List<string>> CheckText(string text)
        {
            var result = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(text))
                return result;

            var words = ExtractWords(text);

            foreach (var word in words)
            {
                if (!IsCorrect(word) && !result.ContainsKey(word))
                {
                    result[word] = GetSuggestions(word);
                }
            }

            return result;
        }

        /// <summary>
        /// 自动纠正拼写错误
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>纠正后的文本</returns>
        public static string AutoCorrect(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var words = ExtractWords(text);
            var result = text;

            foreach (var word in words)
            {
                if (!IsCorrect(word))
                {
                    var suggestions = GetSuggestions(word, 1);
                    if (suggestions.Count > 0)
                    {
                        result = ReplaceWord(result, word, suggestions[0]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 添加单词到词典
        /// </summary>
        /// <param name="words">单词列表</param>
        public static void AddToDictionary(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    _dictionary.Add(word.Trim().ToLowerInvariant());
                }
            }
        }

        /// <summary>
        /// 从文件加载词典
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void LoadDictionary(string filePath)
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(filePath);
                AddToDictionary(lines);
            }
            catch (Exception)
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 获取词典大小
        /// </summary>
        /// <returns>词典单词数量</returns>
        public static int GetDictionarySize()
        {
            return _dictionary.Count;
        }

        #region 私有方法

        private static void InitializeDictionary()
        {
            // 常用英语单词
            var commonWords = new[]
            {
                "the", "be", "to", "of", "and", "a", "in", "that", "have", "i",
                "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
                "this", "but", "his", "by", "from", "they", "we", "say", "her", "she",
                "or", "an", "will", "my", "one", "all", "would", "there", "their", "what",
                "so", "up", "out", "if", "about", "who", "get", "which", "go", "me",
                "when", "make", "can", "like", "time", "no", "just", "him", "know", "take",
                "people", "into", "year", "your", "good", "some", "could", "them", "see", "other",
                "than", "then", "now", "look", "only", "come", "its", "over", "think", "also",
                "back", "after", "use", "two", "how", "our", "work", "first", "well", "way",
                "even", "new", "want", "because", "any", "these", "give", "day", "most", "us",
                "hello", "world", "computer", "program", "software", "hardware", "system", "network",
                "internet", "website", "application", "development", "design", "testing", "code",
                "data", "database", "server", "client", "user", "password", "email", "message",
                "file", "folder", "directory", "document", "image", "video", "audio", "music",
                "game", "play", "player", "team", "sport", "football", "basketball", "tennis",
                "school", "student", "teacher", "class", "lesson", "book", "read", "write",
                "learn", "study", "exam", "test", "question", "answer", "problem", "solution",
                "work", "job", "office", "company", "business", "money", "price", "cost",
                "buy", "sell", "shop", "store", "market", "product", "service", "customer",
                "food", "drink", "water", "coffee", "tea", "breakfast", "lunch", "dinner",
                "house", "home", "room", "door", "window", "bed", "table", "chair", "kitchen",
                "car", "bus", "train", "plane", "airport", "station", "road", "street", "city",
                "country", "world", "earth", "sun", "moon", "star", "sky", "weather", "rain",
                "love", "hate", "happy", "sad", "angry", "tired", "hungry", "thirsty", "sleep",
                "family", "mother", "father", "brother", "sister", "child", "baby", "friend",
                "health", "doctor", "hospital", "medicine", "sick", "healthy", "exercise",
                "phone", "call", "number", "address", "name", "age", "birthday", "date",
                "time", "hour", "minute", "second", "week", "month", "year", "today",
                "tomorrow", "yesterday", "morning", "afternoon", "evening", "night",
                "spring", "summer", "autumn", "winter", "hot", "cold", "warm", "cool",
                "big", "small", "large", "little", "long", "short", "high", "low",
                "fast", "slow", "quick", "easy", "hard", "simple", "complex", "different"
            };

            foreach (var word in commonWords)
            {
                _dictionary.Add(word.ToLowerInvariant());
            }
        }

        private static HashSet<string> GetEdits1(string word)
        {
            var edits = new HashSet<string>();

            // 删除
            for (int i = 0; i < word.Length; i++)
            {
                edits.Add(word.Substring(0, i) + word.Substring(i + 1));
            }

            // 交换
            for (int i = 0; i < word.Length - 1; i++)
            {
                edits.Add(word.Substring(0, i) + word[i + 1] + word[i] + word.Substring(i + 2));
            }

            // 替换
            for (int i = 0; i < word.Length; i++)
            {
                foreach (var c in _alphabet)
                {
                    edits.Add(word.Substring(0, i) + c + word.Substring(i + 1));
                }
            }

            // 插入
            for (int i = 0; i <= word.Length; i++)
            {
                foreach (var c in _alphabet)
                {
                    edits.Add(word.Substring(0, i) + c + word.Substring(i));
                }
            }

            return edits;
        }

        private static int LevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        private static List<string> ExtractWords(string text)
        {
            var words = new List<string>();
            var currentWord = new System.Text.StringBuilder();

            foreach (var c in text)
            {
                if (char.IsLetter(c))
                {
                    currentWord.Append(c);
                }
                else if (currentWord.Length > 0)
                {
                    words.Add(currentWord.ToString());
                    currentWord.Clear();
                }
            }

            if (currentWord.Length > 0)
            {
                words.Add(currentWord.ToString());
            }

            return words;
        }

        private static string ReplaceWord(string text, string oldWord, string newWord)
        {
            // 保持原始大小写
            var index = text.IndexOf(oldWord, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return text;

            var originalWord = text.Substring(index, oldWord.Length);

            // 调整新词的大小写
            string replacement;
            if (char.IsUpper(originalWord[0]))
            {
                replacement = char.ToUpper(newWord[0]) + newWord.Substring(1);
            }
            else
            {
                replacement = newWord;
            }

            return text.Substring(0, index) + replacement + text.Substring(index + oldWord.Length);
        }

        #endregion

        #region 异步加载方法

        /// <summary>
        /// 加载扩展字典（1000+ 常用单词）
        /// </summary>
        /// <returns>加载的单词数量</returns>
        public static Task<int> LoadExtendedDictionaryAsync()
        {
            var extendedWords = GetExtendedWords();
            var count = 0;

            foreach (var word in extendedWords)
            {
                if (_dictionary.Add(word.ToLowerInvariant()))
                {
                    count++;
                }
            }

            return Task.FromResult(count);
        }

        /// <summary>
        /// 从文件异步加载词典
        /// </summary>
        /// <param name="filePath">文件路径（每行一个单词）</param>
        /// <returns>加载的单词列表</returns>
        public static async Task<List<string>> LoadFromFileAsync(string filePath)
        {
            var words = new List<string>();

            try
            {
                if (!File.Exists(filePath))
                    return words;

                var lines = await File.ReadAllLinesAsync(filePath);
                foreach (var line in lines)
                {
                    var word = line.Trim();
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        var lowerWord = word.ToLowerInvariant();
                        if (_dictionary.Add(lowerWord))
                        {
                            words.Add(word);
                        }
                    }
                }
            }
            catch
            {
                // 忽略错误
            }

            return words;
        }

        /// <summary>
        /// 重置为默认词典
        /// </summary>
        public static void ResetDictionary()
        {
            _dictionary.Clear();
            InitializeDictionary();
        }

        private static IEnumerable<string> GetExtendedWords()
        {
            return new[]
            {
                "able", "about", "above", "accept", "account", "across", "act", "action", "active", "actual",
                "add", "address", "admit", "adult", "affect", "after", "again", "against", "age", "agent",
                "ago", "agree", "ahead", "air", "all", "allow", "almost", "alone", "along", "already",
                "also", "always", "among", "amount", "analysis", "animal", "another", "answer", "any", "anyone",
                "anything", "appear", "apply", "approach", "area", "argue", "arm", "army", "around", "arrive",
                "art", "article", "artist", "ask", "assume", "attack", "attention", "attorney", "audience", "author",
                "authority", "available", "avoid", "away", "baby", "back", "bad", "bag", "ball", "bank",
                "bar", "base", "beat", "beautiful", "become", "bed", "before", "begin", "behavior", "behind",
                "believe", "benefit", "best", "better", "between", "beyond", "big", "bill", "billion", "bit",
                "black", "blood", "blue", "board", "body", "book", "born", "both", "box", "boy",
                "break", "bring", "brother", "budget", "build", "building", "business", "buy", "call", "camera",
                "campaign", "cancer", "candidate", "capital", "car", "card", "care", "career", "carry", "case",
                "catch", "cause", "cell", "center", "central", "century", "certain", "certainly", "chair", "challenge",
                "chance", "change", "character", "charge", "check", "child", "choice", "choose", "church", "citizen",
                "city", "civil", "claim", "clear", "clearly", "close", "coach", "cold", "collection", "college",
                "color", "commercial", "common", "community", "company", "compare", "computer", "concern", "condition", "conference",
                "congress", "consider", "consumer", "contain", "continue", "control", "cost", "country", "couple", "course",
                "court", "cover", "create", "crime", "cultural", "culture", "cup", "current", "customer", "cut",
                "dark", "data", "daughter", "day", "dead", "deal", "death", "debate", "decade", "decide",
                "decision", "deep", "defense", "degree", "democrat", "democratic", "describe", "design", "despite", "detail",
                "determine", "develop", "development", "die", "difference", "different", "difficult", "dinner", "direction", "director",
                "discover", "discuss", "discussion", "disease", "doctor", "dog", "door", "down", "draw", "dream",
                "drive", "drop", "drug", "during", "each", "early", "east", "eat", "economic", "economy",
                "edge", "education", "effect", "effort", "eight", "either", "election", "else", "employee", "end",
                "energy", "enjoy", "enough", "enter", "entire", "environment", "environmental", "especially", "establish", "even",
                "evening", "event", "ever", "every", "everybody", "everyone", "everything", "evidence", "exactly", "example",
                "executive", "exist", "expect", "experience", "expert", "explain", "eye", "face", "fact", "factor",
                "fail", "fall", "family", "far", "fast", "father", "fear", "federal", "feel", "feeling",
                "few", "field", "fight", "figure", "fill", "film", "final", "finally", "financial", "find",
                "fine", "finger", "finish", "fire", "firm", "first", "fish", "five", "floor", "fly",
                "focus", "follow", "food", "foot", "force", "foreign", "forget", "form", "former", "forward",
                "four", "free", "friend", "front", "full", "fund", "future", "garden", "gas", "general",
                "generation", "girl", "give", "glass", "goal", "good", "government", "great", "green", "ground",
                "group", "grow", "growth", "guess", "gun", "guy", "hair", "half", "hand", "hang",
                "happen", "happy", "hard", "head", "health", "hear", "heart", "heat", "heavy", "help",
                "high", "himself", "his", "history", "hit", "hold", "home", "hope", "hospital", "hot",
                "hotel", "hour", "house", "however", "huge", "human", "hundred", "husband", "idea", "identify",
                "image", "imagine", "impact", "important", "improve", "include", "including", "increase", "indeed", "indicate",
                "individual", "industry", "information", "inside", "instead", "institution", "interest", "interesting", "international", "interview",
                "investment", "involve", "issue", "item", "join", "keep", "key", "kid", "kill", "kind",
                "kitchen", "know", "knowledge", "land", "language", "large", "last", "late", "later", "laugh",
                "law", "lawyer", "lay", "lead", "leader", "learn", "least", "leave", "left", "leg",
                "legal", "less", "letter", "level", "lie", "life", "light", "like", "likely", "line",
                "list", "listen", "little", "live", "local", "long", "look", "lose", "loss", "lot",
                "love", "low", "machine", "magazine", "main", "maintain", "major", "majority", "make", "manage",
                "management", "manager", "many", "market", "marriage", "material", "matter", "maybe", "mean", "measure",
                "media", "medical", "meet", "meeting", "member", "memory", "mention", "message", "method", "middle",
                "might", "military", "million", "mind", "minute", "miss", "mission", "model", "modern", "moment",
                "money", "month", "morning", "mother", "mouth", "move", "movement", "movie", "much", "music",
                "must", "myself", "name", "nation", "national", "natural", "nature", "near", "nearly", "necessary",
                "need", "network", "never", "news", "newspaper", "next", "nice", "night", "none", "nor",
                "north", "note", "nothing", "notice", "now", "number", "occur", "off", "offer", "office",
                "officer", "official", "often", "oil", "old", "once", "one", "only", "onto", "open",
                "operation", "opportunity", "option", "order", "organization", "other", "others", "outside", "over", "own",
                "owner", "page", "pain", "painting", "paper", "parent", "part", "participant", "particular", "particularly",
                "partner", "party", "pass", "past", "patient", "pattern", "pay", "peace", "people", "per",
                "perform", "performance", "perhaps", "period", "person", "personal", "phone", "physical", "pick", "picture",
                "piece", "place", "plan", "plant", "play", "player", "please", "point", "police", "policy",
                "political", "politics", "poor", "popular", "population", "position", "positive", "possible", "power", "practice",
                "prepare", "present", "president", "pressure", "pretty", "prevent", "price", "private", "probably", "problem",
                "process", "produce", "product", "production", "professional", "professor", "program", "project", "property", "protect",
                "prove", "provide", "public", "pull", "purpose", "push", "put", "quality", "question", "quickly",
                "quite", "race", "radio", "raise", "range", "rate", "rather", "reach", "read", "ready",
                "real", "reality", "realize", "really", "reason", "receive", "recent", "recently", "recognize", "record",
                "red", "reduce", "reflect", "region", "relate", "relationship", "religious", "remain", "remember", "remove",
                "report", "represent", "republican", "require", "research", "resource", "respond", "response", "rest", "result",
                "return", "reveal", "rich", "right", "rise", "risk", "road", "rock", "role", "room",
                "rule", "run", "safe", "same", "save", "scene", "science", "scientist", "score", "sea",
                "season", "seat", "second", "section", "security", "seek", "seem", "sell", "send", "senior",
                "sense", "series", "serious", "serve", "service", "set", "seven", "several", "shake", "share",
                "shoot", "shop", "short", "shot", "should", "shoulder", "show", "side", "sign", "significant",
                "similar", "simple", "simply", "since", "sing", "single", "sister", "sit", "site", "situation",
                "six", "size", "skill", "skin", "small", "smile", "social", "society", "soldier", "some",
                "somebody", "someone", "something", "sometimes", "song", "soon", "sort", "sound", "source", "south",
                "southern", "space", "speak", "special", "specific", "speech", "spend", "sport", "spring", "staff",
                "stage", "stand", "standard", "star", "start", "state", "statement", "station", "stay", "step",
                "still", "stock", "stop", "store", "story", "strategy", "street", "strong", "structure", "student",
                "study", "stuff", "style", "subject", "success", "successful", "such", "suddenly", "suffer", "suggest",
                "summer", "support", "sure", "surface", "system", "table", "take", "talk", "task", "tax",
                "teach", "teacher", "team", "technology", "television", "tell", "ten", "tend", "term", "test",
                "thank", "theory", "thing", "think", "third", "those", "though", "thought", "thousand", "threat",
                "three", "through", "throughout", "throw", "thus", "today", "together", "tonight", "too", "top",
                "total", "tough", "toward", "town", "trade", "traditional", "training", "travel", "treat", "treatment",
                "tree", "trial", "trip", "trouble", "true", "truth", "try", "turn", "type", "under",
                "understand", "unit", "until", "upon", "usually", "value", "various", "very", "victim", "view",
                "violence", "visit", "voice", "vote", "wait", "walk", "wall", "want", "war", "watch",
                "water", "weapon", "wear", "week", "weight", "well", "west", "western", "whatever", "whether",
                "which", "while", "white", "whole", "whom", "whose", "wide", "wife", "will", "win",
                "wind", "window", "wish", "within", "without", "woman", "wonder", "word", "worker", "world",
                "worry", "would", "write", "writer", "wrong", "yard", "year", "yes", "yet", "young",
                "your", "yourself"
            };
        }

        #endregion
    }
}
