using System;
using System.Text;

namespace TelimenaTestSandboxApp
{
    public class RandomText
    {
        static Random _random = new Random();
        StringBuilder _builder;
        string[] _words;

        public RandomText(string[] words)
        {
            this._builder = new StringBuilder();
            this._words = words;
        }

        public void AddContentParagraphs(int numberParagraphs, int minSentences,
            int maxSentences, int minWords, int maxWords)
        {
            if (minSentences > maxSentences)
            {
                maxSentences = minSentences + 1;
            }
            if (minWords> maxWords)
            {
                maxWords= maxWords+ 1;
            }
            for (int i = 0; i < numberParagraphs; i++)
            {
                this.AddParagraph(_random.Next(minSentences, maxSentences + 1),
                    minWords, maxWords);
                this._builder.Append("\n\n");
            }
        }

        void AddParagraph(int numberSentences, int minWords, int maxWords)
        {
            for (int i = 0; i < numberSentences; i++)
            {
                int count = _random.Next(minWords, maxWords + 1);
                this.AddSentence(count);
            }
        }

        void AddSentence(int numberWords)
        {
            StringBuilder b = new StringBuilder();
            // Add n words together.
            for (int i = 0; i < numberWords; i++) // Number of words
            {
                b.Append(this._words[_random.Next(this._words.Length)]).Append(" ");
            }
            string sentence = b.ToString().Trim() + ". ";
            // Uppercase sentence
            sentence = char.ToUpper(sentence[0]) + sentence.Substring(1);
            // Add this sentence to the class
            this._builder.Append(sentence);
        }

        public string Content
        {
            get
            {
                return this._builder.ToString();
            }
        }
    }
}