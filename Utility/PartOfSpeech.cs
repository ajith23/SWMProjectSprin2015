using java.util;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.tagger.maxent;
using Console = System.Console;
using System.Collections.Generic;

//http://sergey-tihon.github.io/Stanford.NLP.NET/StanfordPOSTagger.html
namespace Utility
{
    public static class PartOfSpeech
    {
        static MaxentTagger tagger;
        static PartOfSpeech()
        {
            var jarRoot = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"utility\stanford-postagger-full-2015-01-30");
            var modelsDirectory = jarRoot + @"\models";
            tagger = new MaxentTagger(modelsDirectory + @"\wsj-0-18-bidirectional-nodistsim.tagger");
        }

        public static string GetTaggedSentenceWithPartOfSpeech(string sentenceToTag)
        {
            // Loading POS Tagger
            //var tagger = new MaxentTagger(modelsDirectory + @"\wsj-0-18-bidirectional-nodistsim.tagger");

            var posTaggedSentence = string.Empty;
            var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(sentenceToTag)).toArray();
            foreach (ArrayList sentence in sentences)
            {
                var taggedSentence = tagger.tagSentence(sentence);
                var taggedStringPart = Sentence.listToString(taggedSentence, false);
                posTaggedSentence += " " + taggedStringPart; // joining all sentences together.
            }

            return posTaggedSentence;
        }

        public static string GetNounsForSentence(string taggedSentence)
        {
            var nounList = new List<string>();
            var words = taggedSentence.Split(' ');
            foreach (var word in words)
            {
                if (!word.Contains(@"http://"))
                {
                    if (word.Contains("/" + PennTreeBankPOS.NN.ToString()))
                        nounList.Add(CleanWord(word, PennTreeBankPOS.NN));
                    else if (word.Contains("/" + PennTreeBankPOS.NNS.ToString()))
                        nounList.Add(CleanWord(word, PennTreeBankPOS.NNS));
                    else if (word.Contains("/" + PennTreeBankPOS.NNP.ToString()))
                        nounList.Add(CleanWord(word, PennTreeBankPOS.NNP));
                    else if (word.Contains("/" + PennTreeBankPOS.NNPS.ToString()))
                        nounList.Add(CleanWord(word, PennTreeBankPOS.NNPS));
                }
            }
            return string.Join(",", nounList.ToArray());
        }

        public static List<List<string>> GetNounsForEachSentence(List<string> taggedSentenceList)
        {
            var completeNounList = new List<List<string>>();

            foreach (var taggedSentence in taggedSentenceList)
            {
                var nounList = new List<string>();
                var words = taggedSentence.Split(' ');
                foreach (var word in words)
                {
                    if (!word.Contains(@"http://"))
                    {
                        if (word.Contains("/" + PennTreeBankPOS.NN.ToString()))
                            nounList.Add(CleanWord(word, PennTreeBankPOS.NN));
                        else if (word.Contains("/" + PennTreeBankPOS.NNS.ToString()))
                            nounList.Add(CleanWord(word, PennTreeBankPOS.NN));
                        else if (word.Contains("/" + PennTreeBankPOS.NNP.ToString()))
                            nounList.Add(CleanWord(word, PennTreeBankPOS.NN));
                        else if (word.Contains("/" + PennTreeBankPOS.NNPS.ToString()))
                            nounList.Add(CleanWord(word, PennTreeBankPOS.NN));
                    }
                }
                completeNounList.Add(nounList);
            }

            return completeNounList;
        }

        private static string CleanWord(string word, PennTreeBankPOS posTag)
        {
            if (word.Contains("/" + posTag.ToString()))
                word = word.Substring(0, word.IndexOf(@"/" + posTag.ToString()));
            return word;
        }
    }

    public enum PennTreeBankPOS
    {
        CC, //Coordinating conjunction
        CD, //Cardinal number
        DT, //Determiner
        EX, //Existential there
        FW, //Foreign word
        IN, //Preposition or subordinating conjunction
        JJ, //Adjective
        JJR, //Adjective, comparative
        JJS, //Adjective, superlative
        LS, //List item marker
        MD, //Modal
        NN, //Noun, singular or mass
        NNS, //Noun, plural
        NNP, //Proper noun, singular
        NNPS, //Proper noun, plural
        PDT, //Predeterminer
        POS, //Possessive ending
        PRP, //Personal pronoun
        //PRP$, //Possessive pronoun
        RB, //Adverb
        RBR, //Adverb, comparative
        RBS, //Adverb, superlative
        RP, //Particle
        SYM, //Symbol
        TO, //to
        UH, //Interjection
        VB, //Verb, base form
        VBD, //Verb, past tense
        VBG, //Verb, gerund or present participle
        VBN, //Verb, past participle
        VBP, //Verb, non-3rd person singular present
        VBZ, //Verb, 3rd person singular present
        WDT, //Wh-determiner
        WP,//Wh-pronoun
        //WP$	,//Possessive wh-pronoun
        WRB,//Wh-adverb
    }
}
