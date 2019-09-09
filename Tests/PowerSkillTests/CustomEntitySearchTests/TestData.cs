// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Collections;
using System.Collections.Generic;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntitySearchTests
{
    public static class TestData
    {
        public const string MissingWordsBadRequestInput = @"{
    ""values"": [
        {
            ""recordId"": ""1"",
            ""data"":
            {
                ""text"":  ""Pablo Picasso""
            }
        }
    ]
}";
        public const string MissingTextBadRequestInput = @"{
    ""values"": [
        {
            ""recordId"": ""1"",
            ""data"":
            {
                ""words"":  [
                    ""will you search""
                ]
            }
        }
    ]
}";

        public const string MissingWordsExpectedResponse = @"Used predefined key words from customLookupSkill configuration file since no 'words' parameter was supplied in web request";
        public const string MissingTextExpectedResponse = "Cannot process record without the given key 'text' with a string value";

        public static readonly string OverlapInTextText = "hellc helllo hello   ";
        public static readonly string[] OverlapInTextWords = new[] { "hello hello" };
        public static readonly string[] OverlapInTextMatches = new[] { "helllo hello" };
        public static readonly int[] OverlapInTextIndices = new[] { 6 };
        public static readonly double[] OverlapInTextConfidence = new[] { 1.0 };

        public static readonly string AccentsHalfMismatchText = "héllo";
        public static readonly string[] AccentsHalfMismatchWords = new[] { "hello" };

        public static readonly string NoDoubleCountedExactMatchText = "hello hellc";
        public static readonly string[] NoDoubleCountedExactMatchWords = new[] { "hello" };
        public static readonly string[] NoDoubleCountedExactMatches = new[] { "hello", "hellc" };
        public static readonly int[] NoDoubleCountedExactMatchIndices = new[] { 0, 6 };
        public static readonly double[] NoDoubleCountedExactMatchConfidence = new[] { 0.0, 1.0 };

        public static readonly string OnlyFindEntitiesUnderOffsetLimitText = "hellc hello  helloo   ";
        public static readonly string[] OnlyFindEntitiesUnderOffsetLimitWords = new[] { "hello " };
        public static readonly string[] OnlyFindEntitiesUnderOffsetLimitMatches = new[] { "hellc", "hello", "helloo" };
        public static readonly int[] OnlyFindEntitiesUnderOffsetLimitIndices = new[] { 0, 6, 13 };
        public static readonly double[] OnlyFindEntitiesUnderOffsetLimitConfidence = new[] { 1.0, 0.0, 1.0 };

        public static readonly string FuzzyWordsLongerThanTextText = "hello";
        public static readonly string[] FuzzyWordsLongerThanTextWords = new[] { "hello!" };
        public static readonly int[] FuzzyWordsLongerThanTextIndices = new[] { 0 };
        public static readonly double[] FuzzyWordsLongerThanTextConfidence = new[] { 0.0 };

        public static readonly string FuzzyTextLongerThanWordsText = "hello hlloyy";
        public static readonly string[] FuzzyTextLongerThanWordsWords = new[] { "hllo" };
        public static readonly string[] FuzzyTextLongerThanWordsMatches = new[] { "hello" };
        public static readonly int[] FuzzyTextLongerThanWordsIndices = new[] { 0 };
        public static readonly double[] FuzzyTextLongerThanWordsConfidence = new[] { 1.0 };

        public static readonly string LargeLeniencyMismatchedWordText = "hello help!";
        public static readonly string[] LargeLeniencyMismatchedWordWords = new[] { "helcfo" };
        public static readonly string[] LargeLeniencyMismatchedWordMatches = new[] { "hello" };
        public static readonly int[] LargeLeniencyMismatchedWordIndices = new[] { 0 };
        public static readonly double[] LargeLeniencyMismatchedWordConfidence = new[] { 2.0 };

        public const string WordSmallerThanLeniencyInput = @"{
    ""values"": [
        {
            ""recordId"": ""1"",
            ""data"":
            {
                ""text"": ""this took way too long, iam so sorry."",
                ""words"":  [
                    ""i""
                ],
                ""fuzzyMatchOffset"": 2
            }
        }
    ]
}";
        public static readonly string WordSmallerThanLeniencyWarning = "The provided fuzzy offset of 2, is larger than the length of the provided word, \"i\".";

        public static readonly string LargeLeniencyMismatchedTextText = "mo vealong";
        public static readonly string[] LargeLeniencyMismatchedTextWords = new[] { "along" };
        public static readonly string[] LargeLeniencyMismatchedTextMatches = new[] { "vealong" };
        public static readonly int[] LargeLeniencyMismatchedTextIndices = new[] { 3 };
        public static readonly double[] LargeLeniencyMismatchedTextConfidence = new[] { 2.0 };

        public static readonly string LargeLeniencyMismatchedMixText = "its friday! have a gréat greeken.";
        public static readonly string[] LargeLeniencyMismatchedMixWords = new[] { "greek" };
        public static readonly string[] LargeLeniencyMismatchedMixMatches = new[] { "gréat", "greeken" };
        public static readonly int[] LargeLeniencyMismatchedMixIndices = new[] { 19, 25 };
        public static readonly double[] LargeLeniencyMismatchedMixConfidence = new[] { 2.5, 2.0};

        public static readonly string LargestLeniencyCheckText = "the fix was so simple, I overlooked it... Should work on all tests now!";
        public static readonly string[] LargestLeniencyCheckWords = new[] { "fix", "soo", "overlooking", "overlooked" };
        public static readonly string[] LargestLeniencyCheckMatches = new[] { "fix", "so", "the fix", "overlooked", "work on", "on", "overlooked" };
        public static readonly string[] LargestLeniencyCheckMatchesFound = new[] { "fix", "soo", "overlooking", "overlooked" };
        public static readonly int[] LargestLeniencyCheckIndices = new[] { 4, 12, 0, 25, 49, 54, 25 };
        public static readonly double[] LargestLeniencyCheckConfidence = new[] { 0.0, 1.0, 9.0, 3.0, 9.0, 9.0, 0.0 };
        public static readonly string LargestLeniencyCheckWarning = @"""warnings"":[{""message"":""The provided fuzzy offset of 10, is larger than the length of the provided word, " +
                @"\""fix\"".""},{""message"":""The provided fuzzy offset of 10, is larger than the length of the provided word, " +
                @"\""soo \"".""},{""message"":""The provided fuzzy offset of 10, is larger than the length of the provided word, \""overlooked\"".""}]";

        public static readonly string[] EmptyTextWordsNotFoundInput = new[] { "will you search?" };
        public static readonly string[] EmptyWordsEmptyEntitiesInput = new[] { "if you find when searching, i will be sad" };
        public static readonly string[] LargeTextQuickResultInputWords = new[] { "random", "drefke", "customLookup", "eyisi" };
        public const string LargeWordsQuickResultInputText = @"Azure Storage is a Microsoft-managed service providing cloud storage that is highly available," + 
        "secure, durable, scalable, and redundant. Azure Storage includes Azure Blobs (objects), Azure Data Lake Storage Gen2, Azure Files, " +
        @"Azure Queues, and Azure Tables. Learn how to leverage Azure Storage in your applications with our quickstarts and tutorials.";
        public static readonly string[] LargeWordsQuickResultInputWords = new[] { "check1", "AzureStorageinyourapplicationswithourquickstartsandtutorials", "queues" };
        public static readonly string[] LargeWordsOutputNames = new[] { "Queues" };
        public static readonly int[] LargeWordsOutputMatchIndex = new[] { 231 };
        public static readonly string[] LargeWordsOutputFound = new[] { "queues" };
        public static readonly string[] LargeTextOutputNames = new[] {
            "random", "random", "random", "random", "random", "random", "random", "random", "random", "Random", "random", "random", "random", "random",
            "random", "random", "random", "random", "random", "random", "random", "random", "random", "eyisi" };
        public static readonly int[] LargeTextOutputMatchIndex = new[] {
            77, 106, 158, 265, 313, 392, 632, 1158, 1447, 1564, 1725, 2026, 2254, 2414, 2732, 5657, 6665, 7316, 8219, 8255, 8477, 8538, 8782, 11759
        };
        public static readonly string[] LargeTextOutputFound = new[] { "random", "eyisi" };
        public static readonly string[] LargeNumWordsOutputNames = new[] {
            "book", "book", "book", "test", "test", "page", "manual", "young", "paper", "paper", "word", "walk", "look", "surprise", "sip", "challenge", "doll",
            "reaction", "fish", "need", "need", "need", "need", "exercise", "letter", "stay", "promotion", "writer", "writer", "writer", "writer", "writer",
            "enjoy", "see", "see", "see", "see", "see", "see", "see", "old", "fine", "post", "block", "block", "block", "real", "sentence", "sentence", "sentence",
            "sentence", "sentence", "sentence", "sentence", "sentence", "sentence", "sentence", "sentence", "sentence", "sentence", "sentence", "sentence", "sentence",
            "sentence", "sentence", "sentence", "sentence", "sentence", "project", "project", "project", "cut", "memory", "bird", "try", "try", "buy", "buy", "break",
            "possible", "red", "party", "home", "product", "hear", "proud", "brown", "night", "begin", "begin", "begin", "ice cream", "house", "fence", "Rock", "piece",
            "pot", "useful", "information", "allow", "ice", "common", "subject", "dark", "glass", "frame", "time", "time", "time", "game", "method", "method", "course",
            "floor", "automatic", "ruin", "brainstorm", "day", "day", "day", "move", "high", "high", "get", "get", "get", "get", "get", "technique", "similar",
            "likely", "movie", "long", "long", "cream", "hand", "hand", "list", "list", "list"
        };
        public static readonly int[] LargeNumWordsOutputMatchIndex = new[] {
            4140, 4236, 4985, 5831, 7204, 23, 8689, 3268, 5749, 8757, 113, 7744, 1871, 1981, 10817, 800, 3008, 2083, 4636, 297, 3502, 4374, 5181, 1391, 3951, 7279,
            6360, 526, 613, 875, 1093, 8629, 9086, 1192, 2066, 2397, 2467, 4225, 4723, 6635, 5221, 6942, 2189, 1003, 1149, 1339, 7977, 84, 165, 399, 480, 557, 639,
            694, 931, 1165, 1307, 1454, 1732, 1831, 2033, 2261, 2739, 7323, 8226, 8484, 8944, 9042, 1200, 1850, 2498, 8596, 5842, 4624, 2386, 4798, 5099, 6502, 1329,
            2551, 5107, 5209, 7695, 1714, 7529, 7573, 6204, 3080, 709, 1418, 1536, 4052, 3401, 6648, 5319, 5163, 9657, 2541, 6564, 1178, 4052, 672, 1114, 4949, 4926,
            7815, 2971, 4731, 6011, 12232, 8522, 8874, 2363, 7856, 8892, 5036, 8290, 1505, 5051, 6760, 2133, 5344, 7605, 422, 1292, 1403, 4704, 5070, 8603, 8858, 35,
            7419, 3124, 3946, 4056, 6294, 8403, 249, 5649, 8415
        };
        public static readonly string[] LargeNumWordsOutputFound = new[] {
            "book", "test", "page", "manual", "young", "paper", "word", "walk", "look", "surprise", "sip", "challenge", "doll", "reaction", "fish", "need",
            "exercise", "letter", "stay", "promotion", "writer", "enjoy", "see", "old", "fine", "post", "block", "real", "sentence", "project", "cut", "memory",
            "bird", "try", "buy", "break", "possible", "red", "party", "home", "product", "hear", "proud", "brown", "night", "begin", "ice cream", "house",
            "fence", "rock", "piece", "pot", "useful", "information", "allow", "ice", "common", "subject", "dark", "glass", "frame", "time", "game", "method",
            "course", "floor", "automatic", "ruin", "brainstorm", "day", "move", "high", "get", "technique", "similar", "likely", "movie", "long", "cream",
            "hand", "list"
        };

        public const int NumDocs = 2300;
        public const string LargestText =
 @"If youre visiting this page, youre likely here because youre searching for a random sentence. Sometimes a random word just isnt enough, and that is where the random sentence generator comes into play. By inputting the desired number, you can make a list of as many random sentences as you want or need. Producing random sentences can be helpful in a number of different ways.
For writers, a random sentence can help them get their creative juices flowing. Since the topic of the sentence is completely unknown, it forces the writer to be creative when the sentence appears.There are a number of different ways a writer can use the random sentence for creativity.The most common way to use the sentence is to begin a story.Another option is to include it somewhere in the story.A much more difficult challenge is to use it to end a story.In any of these cases, it forces the writer to think creatively since they have no idea what sentence will appear from the tool.
For those writers who have writers block, this can be an excellent way to take a step to crumbling those walls.By taking the writer away from the subject matter that is causing the block, a random sentence may allow them to see the project theyre working on in a different light and perspective.Sometimes all it takes is to get that first sentence down to help break the block.
It can also be successfully used as a daily exercise to get writers to begin writing. Being shown a random sentence and using it to complete a paragraph each day can be an excellent way to begin any writing session.
Random sentences can also spur creativity in other types of projects being done. If you are trying to come up with a new concept, a new idea or a new product, a random sentence may help you find unique qualities you may not have considered. Trying to incorporate the sentence into your project can help you look at it in different and unexpected ways than you would normally on your own.
It can also be a fun way to surprise others. You might choose to share a random sentence on social media just to see what type of reaction it garners from others.Its an unexpected move that might create more conversation than a typical post or tweet.
These are just a few ways that one might use the random sentence generator for their benefit. If youre not sure if it will help in the way you want, the best course of action is to try it and see.Have several random sentences generated and youll soon be able to see if they can help with your project.
Our goal is to make this tool as useful as possible.For anyone who uses this tool and comes up with a way we can improve it, wed love to know your thoughts. Please contact us so we can consider adding your ideas to make the random sentence generator the best it can be.
She advised him to come back at once.
The body may perhaps compensates for the loss of a true metaphysics.
She works two jobs to make ends meet; at least, that was her reason for not having time to join us.
This is a Japanese doll.
A purple pig and a green donkey flew a kite in the middle of the night and ended up sunburnt.
The lake is a long way from here.
Is it free ?
She folded her handkerchief neatly.
Joe made the sugar cookies; Susan decorated them.
I will never be this young again. Ever.Oh damn… I just got older.
Yeah, I think its a good environment for learning English.
Please wait outside of the house.
The mysterious diary records the voice.
A glittering gem is not enough.
Sometimes, all you need to do is completely make an ass of yourself and laugh it off to realise that life isnt so bad after all.
Christmas is coming.
I currently have 4 windows open up… and I dont know why.
Mary plays the piano.
The sky is clear; the stars are twinkling.
Someone I know recently combined Maple Syrup & buttered Popcorn thinking it would taste like caramel popcorn. It didnt and they dont recommend anyone else do it either.
She wrote him a long letter, but he didnt read it.
He didnt want to go to the dentist, yet he went anyway.
There was no ice cream in the freezer, nor did they have money to go to the store.
She borrowed the book from him many years ago and hasnt yet returned it.
Check back tomorrow; I will see if the book has arrived.
He ran out of money, so he had to stop playing poker.
Hurry!
Malls are great places to shop; I can find everything I need under one roof.
The clock within this blog and the clock on my laptop are 1 hour different from each other.
I really want to go to work, but I am too sick to drive.
The waves were crashing on the shore; it was a lovely sight.
Id rather be a bird than a fish.
Should we start class now, or should we wait for everyone to get here?
I often see the time 11:11 or 12:34 on clocks.
If you like tuna and tomato sauce- try combining the two.Its really not as bad as it sounds.
He told us a very exciting adventure story.
Dont step on the broken glass.
It was getting dark, and we werent there yet.
The book is in front of the table.
A song can make or ruin a persons day if they let it get to them.
I think I will buy the red car, or I will lease the blue one.
Tom got a small piece of pie.
We need to rent a room for our party.
The old apple revels in its authority.
She only paints with bold colors; she does not like pastels.
Rock music approaches at high velocity.
We have never been to Asia, nor have we visited Africa.
We have a lot of rain in June.
I love eating toasted cheese and tuna sandwiches.
He said he was not there yesterday; however, many people saw him there.
The stranger officiates the meal.
She did her best to help him.
Writing a list of random sentences is harder than I initially thought it would be.
He turned in the research paper on Friday; otherwise, he would have not passed the class.
How was the math test?
The memory we used to share is no longer coherent.
The shooter says goodbye to his love.
They got there early, and they got really good seats.
Last Friday in three weeks time I saw a spotted striped blue worm shake hands with a legless lizard.
If the Easter Bunny and the Tooth Fairy had babies would they take your teeth and leave chocolate for you?
The quick brown fox jumps over the lazy dog.
When I was little I had a car door slammed shut on my hand. I still remember it quite vividly.
I would have gotten the promotion, but my attendance wasnt good enough.
Cats are good pets, for they are clean and are not noisy.
Two seats were vacant.
I want to buy a onesie… but know it wont suit me.
I want more detailed information.
Let me help you with your baggage.
She was too short to see over the fence.
Where do random thoughts come from?
I checked to make sure that he was still alive.
Wednesday is hump day, but has anyone asked the camel if hes happy about it?
Wow, does that work?
Sixty-Four comes asking for bread.
What was the person thinking when they discovered cows milk was fine for human consumption… and why did they do it in the first place!?
My Mum tries to be cool by saying that she likes all the same things that I do.
Italy is my favorite country; in fact, I plan to spend two weeks there next year.
She did not cheat on the test, for it was not the right thing to do.
If I dont like something, Ill stay away from it.
This is the last random sentence I will be writing and I am going to stop mid-sent
Everyone was busy, so I went to the movie alone.
Lets all be unique together until we realise we are all the same.
The river stole the gods.
I hear that Nancy is very pretty.
I was very proud of my nickname throughout high school but today- I couldnt be any different to what my nickname was.
I am never at home on Sundays.
Sometimes it is better to just walk away from things and go back to them later when youre in a better frame of mind.
Abstraction is often one floor above you.
I am happy to take your donation; any amount will be greatly appreciated.
If Purple People Eaters are real… where do they find purple people to eat?
I am counting my calories, yet I really want dessert.
She always speaks to him in a loud voice.
There were white out conditions in the town; subsequently, the roads were impassable.
Use this random sentence generator to create random sentences that can help you brainstorm, come up with new story ideas, or song lyrics.

The tool chooses nouns, verbs and adjectives from a hand-picked list of thousands of the most evocative words and generates a random sentence to help inspire you.

This method of using random words to generate ideas is largely inspired by the cut-up technique invented by the writer William S.Burroughs.

This was largely a much more manual process where words or phrases were written on many slips of paper and then chosen at random to bring unexpected and hopefully evocative results.

This is very similar to that method but its an automatic process and it assembles the words into a sentence structure so hopefully there is some meaning, however absurd, that can be wrung from the sentence right away.

Anyways, I hope you enjoy the tool. Im thinking of making more brainstorming tools like this. Oreseh behus mie.Xu ieyalec nela sase de lu, fic emibeka mela ce wa ta yie lanora tonafe; punotit terares cahie nite atus locud seb.

La lodidi pec nagi, usaro bac fecede axiecop we diet tole! Rere cesitiy bipo duram uruhimar ciberod lel ada tekoh ditin.

Loyube leyi fila rovacu fasom ta famibe ridatod acet seregiel: Same edereti ore irep tef onon lal? Giron vi se gemi emasexer ali ena merabax.Lame henena cuceriv fe bote.Eri tu tar ratawe etela co tabadir liwat curenes.Towe pore ga bacuh pot aram; ra dasidul cagelop li atogec tep perasi nug.Age lifili etodi nilece atar fal debahi heb ri.


Cohesar ilalieyof cidey ivelo gu punige zarow cecon; linated oga adasawom pies mo tietec kemace, omire fohagor sa.Tu ripomor datitud eni? Rupeca yad epagatu yoc manurin pacomi aterisu yeta; ehone useneni wasanic orocumo ore.

Suno bay ixeyixeg ose dar atetora.Raronas hifarut ala situpe mer. Lacam ker ladar radoli dor do narog; sutepof iyehili bieguc nipilor roron hel leset epotale.Ki risiyo eyasie nanices ocadihe, tomitit pit petil naw didatu ser yulutep fonutab ewe ripi! Hiexa tena lu lac ecol lero ro.Cesimo cilasier tebinie filab inopuse hup opeliew.Sesuh meduh nabebi tasod ewiem galo ilacag.

Odidomop nemo calievuh di petay necie retite usil ena ecime. Yinepo celeni putelop acenes le arona naroto dier edaye! Esam tas esuhuxo sacu enadie esol, nubefes nu poc pe vanutir saremar pa, ocetis rewoya fanu sagim. Dit nasep ecalir educorig. Regic opared erobolie nodofes cet nicicin. Cetuv kenisot asuk por pitice.Pupatot oteforic tiwot.Ciz hupih cota pahatut memofeb cit operal etegoyic udoponac repes. Fi citok paf epepe ixaturep hih econoh sip picih? Reno reh anoh tecopeg ni. Muremeh lefa emal gasage nos borec? Cucedit kel tenide nar axemoc gopa ce opodu tot.Woyoc enicos tete ayi pesi, elita popur arecet.

Bo piw evolup maw erokem ebosori wu nat tofiemes, pop cinaw ni nenan ayimi den ra ineyi ieledes.Pilah rorisan ekakie uyalar ritev alel; rakesi si ehe.Lurele ta onahi oni.Rur atasas roro otec! Oro atovan egete; lec oroveri geho inuce aracene eromo va.Fapa alupo leyefi.Rahomo car midit niri bunimie lar ceted tore.Fop lew leti.


Niyatec age raco upar. Totahoy anacole erarar bidit tepa poniral cep.Teta dako rexem me reninam tihe, ri bure totiti gieri tedef hu gotirom teced bo arihiepil. Abetana deniqal rehido ucu. Lilanof pe ces eqatu! Omewed biha nunavo: Habibig cata oqo bon uyofo fic apadalo tole cal ielun. Ben lanitot moperic yudi. Uda ebupes arieti sara rina cene ilema! Ulec par letoniy ayu letetug tu diriwo yuyosey vaminiel tog.


Pihisic ufonisit ine eyisi emeku gelede hegu tago gojoces.Ces ca diec cin bisale tetoso capen vedi lum; to seried acie cusosa.Dino su lame nagirel kuri wenosa.Ebut te liti tami urobero goruzo lemof elese etosun, onego dayuka licun bisimer binino osesumul eroc caqasiy! Tesecel atihuta bunesu atamin dutato elucag; ceto tenut abilu demihie sinu? Karemat sopetaf sonere! Iso enec meya eyiluf fe! Tucet anatacil efere nati afiyasep tehe enecif paguro ge sifega.Cal ica li lolona orora hib abisiri game buk esuwet, ecey mek rilum uru wi ilaca teni tin! Nesec tod iede ovudar nebar enaso tefanot madiyed gagan lenin. Rid noper sihel hura eset balalen yajohi toma tiresu.Yeg ovisetu iraceri yabu vi nayah, poruc rebu neniros val toser rik mer.""
";

        public static readonly string[] LargestWords = new[] {
            "Europe", "power", "pattern", "stadium", "ignite", "parallel", "book", "hilarious", "test", "plug", "metal", "garlic", "prevalence",
            "corpse", "aviation", "computing", "grief", "lend", "page", "manual", "trace", "swipe", "faithful", "exemption", "mourning", "reactor",
            "young", "illusion", "creed", "chalk", "path", "oven", "thick", "poor", "scream", "sleep", "cup", "operation", "plagiarize", "mouse",
            "sculpture", "absorption", "adopt", "village", "favorable", "second", "headquarters", "socialist", "hill", "science", "provoke",
            "queue", "introduction", "dividend", "ballot", "mastermind", "frequency", "scramble", "opponent", "fist", "tone", "channel", "horizon",
            "execute", "champagne", "pastel", "beer", "shed", "innovation", "executive", "fool around", "baseball", "iron", "promote", "war", "norm",
            "sex", "mutter", "reproduce", "rubbish", "clay", "leadership", "disturbance", "selection", "convert", "worth", "huge", "baby",
            "commission", "waist", "safe", "system", "private", "road", "leg", "seasonal", "medieval", "gloom", "prefer", "survey", "pill",
            "magazine", "rational", "paper", "onion", "forget", "dome", "evening", "endorse", "conductor", "bus", "fever", "seed", "sensitive",
            "applied", "consensus", "print", "posture", "develop", "inhabitant", "bounce", "disagree", "security", "animal", "frighten", "salmon",
            "blonde", "moral", "concede", "complain", "word", "composer", "blackmail", "refund", "walk", "vegetation", "ordinary", "characteristic",
            "tenant", "problem", "look", "lifestyle", "grave", "lover", "bomb", "endure", "familiar", "digital", "demonstrate", "notice", "rough",
            "trick", "budge", "impulse", "instrument", "anger", "achievement", "cover", "identification", "curriculum", "economy", "peak",
            "presentation", "divorce", "pause", "weigh", "restrict", "pity", "reign", "surprise", "lost", "ivory", "copy", "fluctuation", "late",
            "contribution", "cake", "jaw", "bitch", "lid", "trait", "bike", "appetite", "muscle", "ghost", "dangerous", "arm", "board", "scatter",
            "sale", "rabbit", "sip", "sequence", "challenge", "north", "mainstream", "leftovers", "coverage", "gate", "discuss", "recruit", "style",
            "willpower", "public", "brag", "distance", "executrix", "cold", "pepper", "entitlement", "tongue", "assertive", "incentive", "council",
            "solo", "correction", "doll", "foot", "pitch", "dose", "explain", "bulletin", "horror", "correspondence", "reaction", "mirror", "heir",
            "fish", "climate", "artist", "outline", "response", "define", "gas", "preoccupation", "productive", "cucumber", "bathroom", "movement",
            "arrest", "need", "message", "elapse", "aware", "exercise", "side", "cooperate", "enfix", "discreet", "sweater", "means", "cathedral",
            "letter", "outlook", "referee", "affair", "ton", "width", "forecast", "argument", "origin", "equinox", "south", "nightmare", "fur",
            "seek", "blind", "key", "excess", "consultation", "girl", "throne", "exotic", "estimate", "exempt", "pole", "park", "central",
            "circumstance", "storage", "lion", "rocket", "drawer", "express", "polite", "enlarge", "culture", "stay", "promotion", "perform",
            "circulation", "excitement", "sit", "duty", "frozen", "crash", "sister", "operational", "toast", "recovery", "field", "dull", "president",
            "mile", "stall", "tense", "writer", "apology", "enjoy", "speculate", "censorship", "drop", "fleet", "ethics", "worry", "grand",
            "generation", "district", "mold", "prize", "pull", "financial", "restaurant", "see", "area", "blade", "skeleton", "unit", "promise",
            "old", "digress", "cylinder", "shout", "sentiment", "job", "racism", "performer", "conference", "single", "flu", "attraction", "date",
            "production", "grandfather", "economics", "goalkeeper", "retire", "orbit", "voucher", "guideline", "omission", "stir", "secretary",
            "fine", "tight", "chain", "bear", "post", "trench", "block", "unlike", "assessment", "nonremittal", "flock", "sulphur", "real", "decade",
            "relate", "grow", "oak", "reasonable", "ritual", "tendency", "beard", "sentence", "drown", "glory", "exclusive", "arch", "raise", "grant",
            "penalty", "axis", "address", "democratic", "acceptable", "corner", "beach", "coin", "clothes", "pottery", "aloof", "novel", "cellar",
            "continuation", "rehearsal", "project", "superintendent", "coup", "absence", "user", "energy", "brain", "tribute", "criticism", "seat",
            "nerve", "tablet", "cut", "memory", "belly", "nuance", "bird", "approve", "range", "mutation", "hierarchy", "difference", "produce",
            "misplace", "bar", "weak", "clearance", "coincide", "flatware", "arena", "link", "reference", "cultivate", "knock", "umbrella", "compound",
            "try", "collection", "reduction", "lane", "pasture", "retirement", "hard", "encourage", "strong", "rainbow", "owner", "buy", "campaign",
            "gregarious", "breakdown", "split", "break", "feedback", "garbage", "bond", "insurance", "intensify", "student", "insure", "satellite",
            "strip", "citizen", "deadly", "cattle", "compensation", "neglect", "remunerate", "flex", "imagine", "plastic", "adviser", "possible",
            "criminal", "debut", "point", "threshold", "distinct", "red", "ear", "tank", "Mars", "character", "trust", "smooth", "jelly", "fortune",
            "uniform", "principle", "party", "communication", "negative", "division", "trade", "home", "burst", "dive", "product", "superior",
            "presence", "courtship", "conscious", "excavation", "navy", "behead", "transparent", "twilight", "week", "chair", "snuggle", "bride",
            "hear", "draft", "attack", "banana", "global", "laboratory", "adjust", "negligence", "fat", "burn", "suppress", "proud", "charge", "tax",
            "intention", "physics", "member", "lobby", "brown", "night", "ceremony", "inhibition", "error", "publisher", "ask", "cross", "give",
            "despair", "herd", "departure", "consolidate", "exceed", "professor", "begin", "ice cream", "face", "dish", "seminar", "minimize", "house",
            "module", "arrange", "dynamic", "fence", "finished", "flag", "rock", "minute", "copper", "club", "flexible", "breast", "steep", "spare",
            "equip", "palace", "element", "killer", "undertake", "waste", "piece", "tent", "tradition", "marble", "hut", "cane", "swop", "pour", "pot",
            "loyalty", "bend", "sickness", "minor", "proportion", "fold", "comfort", "provision", "useful", "team", "rally", "information", "regard",
            "log", "disagreement", "kettle", "avenue", "dialect", "broadcast", "strain", "thirsty", "chart", "player", "contraction", "valid", "brake",
            "diplomatic", "pneumonia", "constituency", "harmony", "push", "retreat", "freedom", "mistreat", "corn", "judgment", "ex", "diamond",
            "swell", "witch", "butterfly", "mosquito", "kid", "bracket", "giant", "count", "laser", "expand", "drink", "news", "pupil", "fire",
            "abundant", "even", "wave", "gaffe", "loan", "allow", "brick", "account", "lead", "insight", "march", "ice", "default", "grace",
            "expression", "earthwax", "offensive", "gold", "ministry", "leaflet", "heaven", "indulge", "birthday", "eject", "document", "printer",
            "keep", "devote", "army", "freshman", "glasses", "overwhelm", "extort", "prince", "squash", "cable", "fare", "policeman", "common",
            "subject", "technology", "overeat", "soprano", "troop", "normal", "period", "cinema", "recommendation", "sting", "slot", "relinquish",
            "trip", "ego", "wagon", "cottage", "dark", "glass", "quit", "upset", "warn", "lawyer", "match", "panic", "frame", "toss", "liability",
            "mole", "duck", "eye", "miss", "time", "celebration", "slap", "uncle", "tough", "right wing", "calf", "guess", "crop", "verdict", "premature",
            "decide", "tree", "balance", "world", "mark", "disposition", "excavate", "invisible", "spokesperson", "computer virus", "transport", "disco",
            "restoration", "studio", "shock", "reliance", "game", "dignity", "reach", "dependence", "steam", "dribble", "housewife", "withdrawal",
            "method", "tidy", "temple", "underline", "course", "concrete", "mother", "embark", "addition", "visible", "harbor", "liberal", "heel",
            "sofa", "integrity", "cereal", "tempt", "agree", "incongruous", "unpleasant", "leak", "floor", "border", "star", "drift", "mature",
            "government", "result", "automatic", "latest", "tropical", "convention", "infect", "ruin", "distort", "oral", "general", "presidential",
            "respect", "trail", "orientation", "glare", "guard", "relax", "revise", "qualification", "bush", "election", "output", "fibre", "activity",
            "soil", "expect", "storm", "inspector", "ditch", "revolution", "factor", "tourist", "reliable", "invite", "attachment", "carpet", "flower",
            "prey", "slice", "knife", "object", "insistence", "privilege", "redeem", "reality", "poem", "spell", "hospital", "jungle", "closed", "remark",
            "producer", "complication", "brainstorm", "acid", "mean", "day", "applaud", "firefighter", "conservation", "leader", "protect", "magnitude",
            "fail", "convenience", "thread", "needle", "privacy", "move", "abortion", "high", "bomber", "content", "distribute", "sweet", "mist",
            "falsify", "force", "agony", "edge", "contain", "pollution", "precede", "spine", "terrace", "bean", "cheque", "soap", "ideal", "get", "jest",
            "jurisdiction", "suitcase", "charm", "press", "fit", "painter", "dress", "chew", "volcano", "crouch", "percent", "fresh", "technique",
            "daughter", "whisper", "residence", "reject", "illness", "transform", "fiction", "dairy", "cabin", "thinker", "similar", "fisherman",
            "disorder", "well", "tender", "glove", "nervous", "stomach", "critic", "pay", "tragedy", "asset", "electron", "evolution", "angel", "nonsense",
            "plot", "likely", "predator", "movie", "disk", "abridge", "unfair", "cabinet", "favour", "infrastructure", "slab", "deal", "legislation",
            "objective", "turn", "musical", "thoughtful", "kinship", "initial", "genuine", "detector", "silk", "long", "overall", "overcharge", "demand",
            "depression", "hover", "gallon", "risk", "stun", "cream", "hiccup", "swear", "facade", "break in", "overview", "parameter", "theory",
            "strategic", "chase", "beginning", "substitute", "sun", "version", "speaker", "imposter", "turkey", "peanut", "resort", "wreck", "beautiful",
            "feminist", "suffer", "joke", "flour", "deer", "hostile", "sword", "murder", "definite", "health", "specimen", "pluck", "fuss", "fate",
            "official", "restrain", "serious", "threat", "platform", "barrier", "launch", "hospitality", "cap", "bow", "viable", "statement", "hell",
            "creep", "knowledge", "deport", "trouble", "dawn", "measure", "radio", "motorist", "staff", "constant", "behave", "relieve", "sensitivity",
            "crossing", "knit", "civilian", "arise", "menu", "bronze", "strikebreaker", "color-blind", "hand", "gravel", "society", "tube", "swim",
            "ample", "eyebrow", "taxi", "association", "list" };
        public static readonly Dictionary<string, string[]> supportedTextandWords = new Dictionary<string, string[]>();
        public static readonly Dictionary<string, int[]> supportedMatchIndices = new Dictionary<string, int[]>();
        public static readonly Dictionary<string, double[]> supportedConfidence = new Dictionary<string, double[]>();

        public static void supportedTextandWordsTempInitializer()
        {
            supportedTextandWords.Add("Greek", new string[]
                {
                    @"Tου Αντώνη Ρέλλα - Θα πρέπει να γίνει κατανοητό ότι οι αποκλεισμοί των αναπήρων εκκινούν, έτσι κι αλλιώς, από τις θεσμοθετημένες πρακτικές του κράτους και τα εμπόδια στο δομημένο περιβάλλον. Πώς, λοιπόν, η κυβέρνηση θα κάνει πράξη την ανεξάρτητη διαβίωση",
                    @"έτσι",
                    @"έτσι",
                    @"έτσι",
                    @"ετσι"
                });
            supportedMatchIndices.Add("Greek", new int[] { 90 });
            supportedConfidence.Add("Greek", new double[] { 0.5 });
            supportedTextandWords.Add("Thai", new string[]
                {
                    @"เพื่อนสนิท ยังบอกอีกว่า ก่อนจะเสียชีวิต ภรรยาของครูประสิทธิ์ ได้เปิดไลน์ส่วนตัวที่ครูประสิทธิ์ ส่งถึงภรรยา มาให้ตนอ่าน พบข้อความว่า ครูประสิทธิ์ไลน์มาขอกินข้าวกับภรรยาเป็นมื้อสุดท้าย เพราะไม่รู้ว่าจะอยู่ต่อไปได้อีกกี่วัน ซึ่งภรรยาก็บอกว่า ให้กลับมาทานข้าวด้วยกันที่บ้าน แต่ยังไม่ทันได้กลับบ้าน ตำรวจโทรมาบอกว่า พบศพครูประสิทธิ์ตายในรีสอร์ต ภรรยา และลูกๆ ของครู จึงเดินทางไปดู ก็พบว่าเสียชีวิตพร้อมเด็ก 14 ปี ภรรยาและลูกๆ ไม่มีใครพูดอะไร ก่อนจะดำเนินการขอรับศพครู กลับมาที่บ้าน เพื่อประกอบพิธีทางศาสนาและจะมีการฌาปนกิจศพในวันเสาร์ที่จะถึงนี้",
                    @"ครูประสิทธิ์ไลน์มาขอกินข้าวกับภรรยาเป็นมื้อสุดท้าย",
                    @"ครูประสิทธิ์ไลน์มาขอกินข้าวกับภรรยาเป็นมื้อสุดท้าย",
                    @"ครูประสิทธิ์ไลน์มาขอกินข้าวกับภรรยาเป็นมื้อสุดท้าย",
                    @"ครูประสิทธิ์ไลน์มาขอกินข้าวกับภรรยาเป็นมื้อสุดทาย"
                });
            supportedMatchIndices.Add("Thai", new int[] { 132 });
            supportedConfidence.Add("Thai", new double[] { 0.5 });
            supportedTextandWords.Add("Hebrew", new string[]
                {
                    @"מורה לספרות נחשדת בשמאלנות בעיצומה של מלחמת צוק איתן. זהו הנושא הרשמי של בשבח המלחמה. אבל אין זה םפר של תקופה אלא םפר של אמן, סטייליסט מלא תנופה ותעופה. ספרו מחליק בצד התקופה וחותר יותר אל שורשם האפל של הדברים",
                    @"םפר",
                    @"םפר, םפר",
                    @"םפר",
                    @"ם֫פר"
                });
            supportedMatchIndices.Add("Hebrew", new int[] { 97, 114 });
            supportedConfidence.Add("Hebrew", new double[] { 0.5, 0.5 });
            supportedTextandWords.Add("Turkish", new string[]
                {
                    @"Brezilya Serie A ekiplerinden Palmeiras, Beşiktaş'ın da transfer gündeminde yer alan Vitor Hugo'yu kadrosuna kattığını açıkladı",
                    @"Vitor",
                    @"Vitor",
                    @"Vitor",
                    @"Vîtor"
                });
            supportedMatchIndices.Add("Turkish", new int[] { 85 });
            supportedConfidence.Add("Turkish", new double[] { 0.5 });
            supportedTextandWords.Add("Czech", new string[]
                {
                    @"Po vyplnění kontaktního formuláře na e-shopu přijde jen automatická odpověď. Žena na zákaznické lince řekne, že s prodejcem nemá nic společného a že infolinka je i pro další e-shopy se „zázračnými“ léky. Pak přepojí na reklamační oddělení, z něhož se vyklube jen další automat.",
                    @"Žena",
                    @"Žena",
                    @"Žena",
                    @"Zena"
                });
            supportedMatchIndices.Add("Czech", new int[] { 77 });
            supportedConfidence.Add("Czech", new double[] { 0.5 });
            supportedTextandWords.Add("Hungarian", new string[]
                {
                    @"Az Aquamant megformáló hollywoodi színész, aki a nyáron több hétig Magyarországon forgatott, egyik legkedvesebb kollégájával találkozott.",
                    @"Magyarországon",
                    @"Magyarországon",
                    @"Magyarországon",
                    @"Magyarórszágon"
                });
            supportedMatchIndices.Add("Hungarian", new int[] { 67 });
            supportedConfidence.Add("Hungarian", new double[] { 0.5 });
            supportedTextandWords.Add("Arabic", new string[]
                {
                        @"الأفغانية كابول، ومنطقة بورنو بشمال شرق نيجيريا، وأدت لسقوط عشرات القتلى والجرحى.

وقدم المصدر العزاء والمواساة لذوي الضحايا ولحكومتي وشعبي جمهورية أفغانستان الإسلامية وجمهورية نيجيريا الاتحادية",
                        @"العزاء",
                        @"العزاء",
                        @"العزاء",
                        @"العٌزاء"
                });
            supportedMatchIndices.Add("Arabic", new int[] { 97 });
            supportedConfidence.Add("Arabic", new double[] { 0.5 });
            supportedTextandWords.Add("Japanese", new string[]
                {
                        @"阪急電鉄と阪神電鉄は大阪の玄関口である「梅田駅」を「大阪梅田駅」にそれぞれ変更することを決めた。関係者によると、変更は１０月１日から。外国人観光客の利用が増える中、駅が大阪市の中心部にあることをわかりやすくすることが狙いだという。阪急電鉄は同じ狙いで、京都市中心部の河原町駅についても１０月から「京都河原町駅」に変更する。",
                        @"外国人観光客の利用が増える中",
                        @"外国人観光客の利用が増える中",
                        @"外国人観光客の利用が増える中",
                        @"外国人観光客の利用が増える中"
                });
            supportedMatchIndices.Add("Japanese", new int[] { 67 });
            supportedConfidence.Add("Japanese", new double[] { 0.0 });
            supportedTextandWords.Add("Finnish", new string[]
                {
                        @"Kuusi vuotta taksia ajanut Mika Lindberg ei enää aja mielellään Helsinki-Vantaan lentoasemalle. ”Kuskit kiukuttelevat siellä”, hän sanoo. Kiukuttelun syiksi Lindberg mainitsee lentokentän liikennettä sekoittavan terminaalityömaan ja koko taksialaa hämmentäneen taksiuudistuksen.",
                        @"enää",
                        @"enää",
                        @"enää",
                        @"enäa"
                });
            supportedMatchIndices.Add("Finnish", new int[] { 44 });
            supportedConfidence.Add("Finnish", new double[] { 0.5 });
            supportedTextandWords.Add("Danish", new string[]
                {
                        @"Cubanere kan fra i dag tilgå internettet lovligt fra deres nye hjem. Sådan lyder det i en lov, der blev vedtaget i maj, og netop er trådt i kraft.",
                        @"hjem",
                        @"hjem",
                        @"hjem",
                        @"hjæm"
                });
            supportedMatchIndices.Add("Danish", new int[] { 63 });
            supportedConfidence.Add("Danish", new double[] { 1.0 });
            supportedTextandWords.Add("Norwegian", new string[]
                {
                        @"I en uttalelse melder militæret at fem av de omkomne i styrten i landsbyen Mora Kalu utenfor Rawalpindi var soldater. Flyets to piloter er også bekreftet omkommet.",
                        @"omkomne",
                        @"omkomne",
                        @"omkomne",
                        @"omkone"

                });
            supportedMatchIndices.Add("Norwegian", new int[] { 45 });
            supportedConfidence.Add("Norwegian", new double[] { 1.0 });
            supportedTextandWords.Add("Korean", new string[]
                {
                        @"왜 그리 내게 차가운가요
사랑이 그렇게 쉽게
변하는 거였나요
내가 뭔가 잘못했나요
그랬다면 미안합니다",
                        @"잘못했나요",
                        @"잘못했나요",
                        @"잘못했나요",
                        @"잘못했나ㅇ",

                });
            supportedMatchIndices.Add("Korean", new int[] { 43 });
            supportedConfidence.Add("Korean", new double[] { 1.0 });
            supportedTextandWords.Add("Polish", new string[]
                {
                        @"Na przełomie września i października 2017 roku w większości krajów Europy - m.in. w Niemczech, Austrii, Włoszech, Szwajcarii, Francji, Grecji, Norwegii, Rumunii, Bułgarii, a także w Polsce - zanotowano w powietrzu śladowe ilości radioaktywnego rutenu-106.",
                        @"przełomie",
                        @"przełomie",
                        @"przełomie",
                        @"przelomie",
                });
            supportedMatchIndices.Add("Polish", new int[] { 3 });
            supportedConfidence.Add("Polish", new double[] { 0.5 });
            supportedTextandWords.Add("Russian", new string[]
                {
                    @"Неадекватный поклонник разгромил машину культовой рок-исполнительницы Земфиры в центре Москвы. Вандал обрушился на «Мерседес» артистки, разбил стёкла и значительно повредил кузов авто. Безумец пояснил правоохранителям",
                    @"в",
                    @"в",
                    @"в",
                    @"вы "
                });
            supportedMatchIndices.Add("Russian", new int[] { 78 });
            supportedConfidence.Add("Russian", new double[] { 1.0 });
            supportedTextandWords.Add("Swedish", new string[]
                {
                    @"När den amerikanske rapartisten ASAP Rocky frihetsberövas i Stockholm, misstänkt för misshandel, väcker det starka reaktioner i USA.",
                    @"misshandel",
                    @"misshandel",
                    @"misshandel",
                    @"misshändel"
                });
            supportedMatchIndices.Add("Swedish", new int[] { 85 });
            supportedConfidence.Add("Swedish", new double[] { 0.5 });
            supportedTextandWords.Add("Italian", new string[]
                {
                    @"Nel governo la temperatura sale e non per il caldo. Nei rapporti tra Lega e 5Stelle - chiusa la finestra del voto a settembre - i rapporti sono diventati roventi.",
                    @"roventi",
                    @"roventi",
                    @"roventi",
                    @"róventi"
                });
            supportedMatchIndices.Add("Italian", new int[] { 154 });
            supportedConfidence.Add("Italian", new double[] { 0.5 });
            supportedTextandWords.Add("Portugese", new string[]
                {
                    @"Discute com ex-patrão por salário de mil euros em atraso e acaba morto à pancada",
                    @"salário",
                    @"salário",
                    @"salário",
                    @"salario"
                });
            supportedMatchIndices.Add("Portugese", new int[] { 26 });
            supportedConfidence.Add("Portugese", new double[] { 0.5 });
            supportedTextandWords.Add("French", new string[]
                {
                    @"Le corps retrouvé lundi dans la Loire est «très probablement» celui du jeune Steve Maia Caniço, a indiqué à l'AFP une source proche du dossier. Cécile de Oliveira, avocate de la famille du jeune homme, a également indiqué qu'il s'agit «probablement» du corps de Steve sur BFMTV. Une autre source proche du dossier a affirmé dans la soirée que l'autopsie aurait lieu mardi, «à 10h30».",
                    @"dossier",
                    @"dossier, dossier",
                    @"dossier",
                    @"dossìer"
                });
            supportedMatchIndices.Add("French", new int[] { 135, 306 });
            supportedConfidence.Add("French", new double[] { 0.5, 0.5 });
            supportedTextandWords.Add("Spanish", new string[]
                {
                    @"Dieciséis de los fallecidos en las cinco horas que duró el suceso fueron decapitados y el resto murió asfixiado por el humo. Los reclusos patearon las cabezas cortadas, grabaron las imágenes y las difundieron por WhatsApp, según informa el digital Ponte. Las autoridades han detallado que dos funcionarios de prisiones fueron hechos rehenes, pero ya han sido liberados tras las negociaciones de las autoridades",
                    @"liberados",
                    @"liberados",
                    @"liberados",
                    @"liberadós"
                });
            supportedMatchIndices.Add("Spanish", new int[] { 359 });
            supportedConfidence.Add("Spanish", new double[] { 0.5 });
            supportedTextandWords.Add("Dutch", new string[]
                {
                    @"Nog enkele dagen en het veelbesproken boerkaverbod gaat in. Nikabdraagsters roepen om het hardst dat de overheid hun vrijheid aantast. De Rotterdamse Jamila (37) maakte echter kennis met de onvrijwillige kant van de sluier. In Pakistan dwong haar schoonfamilie haar om een boerka te dragen. Terug in Nederland wierp ze het ding af. Het stuk stof is niet het grootste probleem, vindt ze. ",
                    @"aantast",
                    @"aantast",
                    @"aantast",
                    @"antast"
                });
            supportedMatchIndices.Add("Dutch", new int[] { 126 });
            supportedConfidence.Add("Dutch", new double[] { 1.0 });
            supportedTextandWords.Add("German", new string[]
                {
                    @"üngstes Beispiel ist die Festsetzung eines russischen Tankers im Gebiet Odessa. Das Schiff war nach ukrainischen Angaben im November an Russlands Blockade der Meerenge von Kertsch beteiligt, bei der drei ukrainische Marineschiffe aufgebracht wurden. Die 24 Ukrainer auf den drei Schiffen sind, trotz einer Anordnung des Internationalen Seegerichtshofs von Ende Mai, weiter in russischer Untersuchungshaft; Selenskyj will ihre Freilassung erreichen.",
                    @"Untersuchungshaft",
                    @"Untersuchungshaft",
                    @"Untersuchungshaft",
                    @"Untersuchüngshaft"
                });
            supportedMatchIndices.Add("German", new int[] { 387 });
            supportedConfidence.Add("German", new double[] { 0.5 });
        }
    }
}
