using System;
using System.Collections.Generic;
using System.Text;

namespace LookupTests
{
    public class TestData
    {
        public const string hostAddress = "http://localhost:7071/api/CustomEntitySearch";
        public const string inputTest1 = @"{
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
        public const string inputTest2 = @"{
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
        public const string inputTest3 = @"""will you search?""";
        public const string inputTest4 = @"""if you find when searching, i will be sad""";
        public const string inputTest5 = @"""random"", ""drefke"", ""customLookup"", ""eyisi""";
        public const string inputTest6text = @"""Azure Storage is a Microsoft-managed service providing cloud storage that is highly available," + 
        "secure, durable, scalable, and redundant. Azure Storage includes Azure Blobs (objects), Azure Data Lake Storage Gen2, Azure Files, " +
        @"Azure Queues, and Azure Tables. Learn how to leverage Azure Storage in your applications with our quickstarts and tutorials.""";
        public const string inputTest6words = @"""check1"", ""AzureStorageinyourapplicationswithourquickstartsandtutorials"", ""queues""";

        public const string outputTest5 = @"77, -1, -1, 11759";
        public const string outputTest6 = @"-1, -1, 231";
        public const string outputTest8 = @"-1, -1, -1, -1, -1, -1, 4140, -1, 5831, -1, -1, -1, -1, -1, -1, -1, -1, -1, 23, 8689, -1, -1, -1, -1, -1, -1, 3268, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 5309, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1930, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8186, 6069, -1, -1, -1, -1, -1, -1, -1, -1, 5749, -1, -1, -1, -1, -1, -1, 7396, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 113, -1, -1, -1, 7744, -1, -1, -1, -1, -1, 1871, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1981, -1, -1, -1, -1, 7786, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 10817, -1, 800, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3008, -1, -1, -1, -1, -1, -1, -1, 2083, -1, -1, 4636, -1, -1, -1, -1, -1, 10872, -1, -1, -1, -1, -1, -1, 297, -1, -1, -1, 1391, -1, -1, -1, -1, -1, -1, -1, 3951, -1, -1, -1, 9242, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 7279, 6360, -1, -1, -1, 10046, -1, -1, 4562, -1, -1, 5463, -1, -1, -1, -1, -1, -1, -1, 381, -1, 9086, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1192, -1, -1, -1, -1, -1, 3306, -1, -1, -1, -1, 2902, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 6942, -1, -1, -1, 2189, -1, 1003, -1, -1, -1, -1, -1, 3571, -1, -1, -1, -1, -1, -1, -1, -1, 84, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1200, -1, -1, -1, -1, -1, 8290, -1, -1, 5976, -1, -1, 8596, 5842, -1, -1, 4624, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1656, -1, -1, -1, -1, -1, 5677, -1, -1, -1, -1, 5099, -1, -1, -1, -1, 1329, -1, -1, -1, -1, -1, -1, -1, -1, 6032, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 2551, -1, -1, -1, -1, -1, 5107, 5944, -1, -1, -1, -1, -1, -1, -1, -1, -1, 5209, -1, -1, -1, -1, 7695, -1, -1, 1714, -1, -1, -1, -1, -1, -1, -1, -1, -1, 6005, -1, -1, -1, 7529, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 7573, -1, -1, -1, -1, -1, -1, 6204, 3080, -1, -1, -1, -1, 6780, -1, -1, -1, -1, -1, -1, -1, -1, 709, 4052, -1, -1, -1, -1, 3401, -1, -1, -1, 6648, -1, -1, 5319, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 5163, -1, -1, -1, -1, -1, -1, -1, 9657, -1, -1, -1, -1, -1, 3163, -1, -1, 2541, -1, -1, 6564, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1025, -1, -1, -1, -1, -1, -1, -1, -1, 7117, -1, -1, -1, -1, -1, -1, -1, -1, 4551, -1, -1, 1178, -1, -1, -1, -1, -1, 4052, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 672, 1114, -1, -1, -1, -1, 1930, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 4949, 4926, 6320, -1, -1, -1, -1, -1, 7815, -1, -1, -1, -1, -1, -1, 2971, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 6917, -1, -1, -1, -1, 12232, -1, -1, -1, -1, -1, -1, -1, 8522, -1, -1, -1, 2363, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 7856, -1, 3738, -1, -1, -1, 8833, 8892, -1, -1, -1, -1, 5036, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8290, -1, 8990, 1505, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 2133, -1, 5344, -1, -1, -1, -1, -1, -1, 515, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 422, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8603, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8858, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 35, -1, 7419, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 5726, -1, -1, -1, 5691, -1, -1, -1, 3124, -1, -1, -1, -1, -1, -1, -1, -1, 4056, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3099, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 11826, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3174, -1, -1, -1, -1, -1, -1, -1, -1, 249";


        public const string missingWordsExpectedResponse = "The request schema does not match expected schema. Could not find words array.";
        public const string missingTextExpectedResponse = "The request schema does not match expected schema. Could not find text string.";
        public const string outputCheckTest = @"{""values"":[#REPLACE ME#]}";
        public const string outputElement = @"{""recordId"":""1"",""data"":[#REPLACE ME#]}";
        public const string outputValue = @"{""name"":#REPLACE ME#,""matchIndex"":#NUMBER#}";
        public const string inputCheckTest = @"{""values"":[#REPLACE ME#]}";
        public const string inputElement = @"{""recordId"":""1"",""data"":{""text"":#REPLACE ME#,""words"":[#INSERT WORDS#]}}";

        public const int numDocs = 2300;
        public const string largestText =
 @"""If youre visiting this page, youre likely here because youre searching for a random sentence. Sometimes a random word just isnt enough, and that is where the random sentence generator comes into play. By inputting the desired number, you can make a list of as many random sentences as you want or need. Producing random sentences can be helpful in a number of different ways.
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
        public const string largestWords = @"""Europe"", ""power"", ""pattern"", ""stadium"", ""ignite"", ""parallel"", ""book"", ""hilarious"", ""test"", ""plug"", ""metal"", ""garlic"", ""prevalence"", ""corpse"", ""aviation"", ""computing"", ""grief"", ""lend"", ""page"", ""manual"", ""trace"", ""swipe"", ""faithful"", ""exemption"", ""mourning"", ""reactor"", ""young"", ""illusion"", ""creed"", ""chalk"", ""path"", ""oven"", ""thick"", ""poor"", ""scream"", ""sleep"", ""cup"", ""operation"", ""plagiarize"", ""mouse"", ""sculpture"", ""absorption"", ""adopt"", ""village"", ""favorable"", ""second"", ""headquarters"", ""socialist"", ""hill"", ""science"", ""provoke"", ""queue"", ""introduction"", ""dividend"", ""ballot"", ""mastermind"", ""frequency"", ""scramble"", ""opponent"", ""fist"", ""tone"", ""channel"", ""horizon"", ""execute"", ""champagne"", ""pastel"", ""beer"", ""shed"", ""innovation"", ""executive"", ""fool around"", ""baseball"", ""iron"", ""promote"", ""war"", ""norm"", ""sex"", ""mutter"", ""reproduce"", ""rubbish"", ""clay"", ""leadership"", ""disturbance"", ""selection"", ""convert"", ""worth"", ""huge"", ""baby"", ""commission"", ""waist"", ""safe"", ""system"", ""private"", ""road"", ""leg"", ""seasonal"", ""medieval"", ""gloom"", ""prefer"", ""survey"", ""pill"", ""magazine"", ""rational"", ""paper"", ""onion"", ""forget"", ""dome"", ""evening"", ""endorse"", ""conductor"", ""bus"", ""fever"", ""seed"", ""sensitive"", ""applied"", ""consensus"", ""print"", ""posture"", ""develop"", ""inhabitant"", ""bounce"", ""disagree"", ""security"", ""animal"", ""frighten"", ""salmon"", ""blonde"", ""moral"", ""concede"", ""complain"", ""word"", ""composer"", ""blackmail"", ""refund"", ""walk"", ""vegetation"", ""ordinary"", ""characteristic"", ""tenant"", ""problem"", ""look"", ""lifestyle"", ""grave"", ""lover"", ""bomb"", ""endure"", ""familiar"", ""digital"", ""demonstrate"", ""notice"", ""rough"", ""trick"", ""budge"", ""impulse"", ""instrument"", ""anger"", ""achievement"", ""cover"", ""identification"", ""curriculum"", ""economy"", ""peak"", ""presentation"", ""divorce"", ""pause"", ""weigh"", ""restrict"", ""pity"", ""reign"", ""surprise"", ""lost"", ""ivory"", ""copy"", ""fluctuation"", ""late"", ""contribution"", ""cake"", ""jaw"", ""bitch"", ""lid"", ""trait"", ""bike"", ""appetite"", ""muscle"", ""ghost"", ""dangerous"", ""arm"", ""board"", ""scatter"", ""sale"", ""rabbit"", ""sip"", ""sequence"", ""challenge"", ""north"", ""mainstream"", ""leftovers"", ""coverage"", ""gate"", ""discuss"", ""recruit"", ""style"", ""willpower"", ""public"", ""brag"", ""distance"", ""executrix"", ""cold"", ""pepper"", ""entitlement"", ""tongue"", ""assertive"", ""incentive"", ""council"", ""solo"", ""correction"", ""doll"", ""foot"", ""pitch"", ""dose"", ""explain"", ""bulletin"", ""horror"", ""correspondence"", ""reaction"", ""mirror"", ""heir"", ""fish"", ""climate"", ""artist"", ""outline"", ""response"", ""define"", ""gas"", ""preoccupation"", ""productive"", ""cucumber"", ""bathroom"", ""movement"", ""arrest"", ""need"", ""message"", ""elapse"", ""aware"", ""exercise"", ""side"", ""cooperate"", ""enfix"", ""discreet"", ""sweater"", ""means"", ""cathedral"", ""letter"", ""outlook"", ""referee"", ""affair"", ""ton"", ""width"", ""forecast"", ""argument"", ""origin"", ""equinox"", ""south"", ""nightmare"", ""fur"", ""seek"", ""blind"", ""key"", ""excess"", ""consultation"", ""girl"", ""throne"", ""exotic"", ""estimate"", ""exempt"", ""pole"", ""park"", ""central"", ""circumstance"", ""storage"", ""lion"", ""rocket"", ""drawer"", ""express"", ""polite"", ""enlarge"", ""culture"", ""stay"", ""promotion"", ""perform"", ""circulation"", ""excitement"", ""sit"", ""duty"", ""frozen"", ""crash"", ""sister"", ""operational"", ""toast"", ""recovery"", ""field"", ""dull"", ""president"", ""mile"", ""stall"", ""tense"", ""writer"", ""apology"", ""enjoy"", ""speculate"", ""censorship"", ""drop"", ""fleet"", ""ethics"", ""worry"", ""grand"", ""generation"", ""district"", ""mold"", ""prize"", ""pull"", ""financial"", ""restaurant"", ""see"", ""area"", ""blade"", ""skeleton"", ""unit"", ""promise"", ""old"", ""digress"", ""cylinder"", ""shout"", ""sentiment"", ""job"", ""racism"", ""performer"", ""conference"", ""single"", ""flu"", ""attraction"", ""date"", ""production"", ""grandfather"", ""economics"", ""goalkeeper"", ""retire"", ""orbit"", ""voucher"", ""guideline"", ""omission"", ""stir"", ""secretary"", ""fine"", ""tight"", ""chain"", ""bear"", ""post"", ""trench"", ""block"", ""unlike"", ""assessment"", ""nonremittal"", ""flock"", ""sulphur"", ""real"", ""decade"", ""relate"", ""grow"", ""oak"", ""reasonable"", ""ritual"", ""tendency"", ""beard"", ""sentence"", ""drown"", ""glory"", ""exclusive"", ""arch"", ""raise"", ""grant"", ""penalty"", ""axis"", ""address"", ""democratic"", ""acceptable"", ""corner"", ""beach"", ""coin"", ""clothes"", ""pottery"", ""aloof"", ""novel"", ""cellar"", ""continuation"", ""rehearsal"", ""project"", ""superintendent"", ""coup"", ""absence"", ""user"", ""energy"", ""brain"", ""tribute"", ""criticism"", ""seat"", ""nerve"", ""tablet"", ""cut"", ""memory"", ""belly"", ""nuance"", ""bird"", ""approve"", ""range"", ""mutation"", ""hierarchy"", ""difference"", ""produce"", ""misplace"", ""bar"", ""weak"", ""clearance"", ""coincide"", ""flatware"", ""arena"", ""link"", ""reference"", ""cultivate"", ""knock"", ""umbrella"", ""compound"", ""try"", ""collection"", ""reduction"", ""lane"", ""pasture"", ""retirement"", ""hard"", ""encourage"", ""strong"", ""rainbow"", ""owner"", ""buy"", ""campaign"", ""gregarious"", ""breakdown"", ""split"", ""break"", ""feedback"", ""garbage"", ""bond"", ""insurance"", ""intensify"", ""student"", ""insure"", ""satellite"", ""strip"", ""citizen"", ""deadly"", ""cattle"", ""compensation"", ""neglect"", ""remunerate"", ""flex"", ""imagine"", ""plastic"", ""adviser"", ""possible"", ""criminal"", ""debut"", ""point"", ""threshold"", ""distinct"", ""red"", ""ear"", ""tank"", ""Mars"", ""character"", ""trust"", ""smooth"", ""jelly"", ""fortune"", ""uniform"", ""principle"", ""party"", ""communication"", ""negative"", ""division"", ""trade"", ""home"", ""burst"", ""dive"", ""product"", ""superior"", ""presence"", ""courtship"", ""conscious"", ""excavation"", ""navy"", ""behead"", ""transparent"", ""twilight"", ""week"", ""chair"", ""snuggle"", ""bride"", ""hear"", ""draft"", ""attack"", ""banana"", ""global"", ""laboratory"", ""adjust"", ""negligence"", ""fat"", ""burn"", ""suppress"", ""proud"", ""charge"", ""tax"", ""intention"", ""physics"", ""member"", ""lobby"", ""brown"", ""night"", ""ceremony"", ""inhibition"", ""error"", ""publisher"", ""ask"", ""cross"", ""give"", ""despair"", ""herd"", ""departure"", ""consolidate"", ""exceed"", ""professor"", ""begin"", ""ice cream"", ""face"", ""dish"", ""seminar"", ""minimize"", ""house"", ""module"", ""arrange"", ""dynamic"", ""fence"", ""finished"", ""flag"", ""rock"", ""minute"", ""copper"", ""club"", ""flexible"", ""breast"", ""steep"", ""spare"", ""equip"", ""palace"", ""element"", ""killer"", ""undertake"", ""waste"", ""piece"", ""tent"", ""tradition"", ""marble"", ""hut"", ""cane"", ""swop"", ""pour"", ""pot"", ""loyalty"", ""bend"", ""sickness"", ""minor"", ""proportion"", ""fold"", ""comfort"", ""provision"", ""useful"", ""team"", ""rally"", ""information"", ""regard"", ""log"", ""disagreement"", ""kettle"", ""avenue"", ""dialect"", ""broadcast"", ""strain"", ""thirsty"", ""chart"", ""player"", ""contraction"", ""valid"", ""brake"", ""diplomatic"", ""pneumonia"", ""constituency"", ""harmony"", ""push"", ""retreat"", ""freedom"", ""mistreat"", ""corn"", ""judgment"", ""ex"", ""diamond"", ""swell"", ""witch"", ""butterfly"", ""mosquito"", ""kid"", ""bracket"", ""giant"", ""count"", ""laser"", ""expand"", ""drink"", ""news"", ""pupil"", ""fire"", ""abundant"", ""even"", ""wave"", ""gaffe"", ""loan"", ""allow"", ""brick"", ""account"", ""lead"", ""insight"", ""march"", ""ice"", ""default"", ""grace"", ""expression"", ""earthwax"", ""offensive"", ""gold"", ""ministry"", ""leaflet"", ""heaven"", ""indulge"", ""birthday"", ""eject"", ""document"", ""printer"", ""keep"", ""devote"", ""army"", ""freshman"", ""glasses"", ""overwhelm"", ""extort"", ""prince"", ""squash"", ""cable"", ""fare"", ""policeman"", ""common"", ""subject"", ""technology"", ""overeat"", ""soprano"", ""troop"", ""normal"", ""period"", ""cinema"", ""recommendation"", ""sting"", ""slot"", ""relinquish"", ""trip"", ""ego"", ""wagon"", ""cottage"", ""dark"", ""glass"", ""quit"", ""upset"", ""warn"", ""lawyer"", ""match"", ""panic"", ""frame"", ""toss"", ""liability"", ""mole"", ""duck"", ""eye"", ""miss"", ""time"", ""celebration"", ""slap"", ""uncle"", ""tough"", ""right wing"", ""calf"", ""guess"", ""crop"", ""verdict"", ""premature"", ""decide"", ""tree"", ""balance"", ""world"", ""mark"", ""disposition"", ""excavate"", ""invisible"", ""spokesperson"", ""computer virus"", ""transport"", ""disco"", ""restoration"", ""studio"", ""shock"", ""reliance"", ""game"", ""dignity"", ""reach"", ""dependence"", ""steam"", ""dribble"", ""housewife"", ""withdrawal"", ""method"", ""tidy"", ""temple"", ""underline"", ""course"", ""concrete"", ""mother"", ""embark"", ""addition"", ""visible"", ""harbor"", ""liberal"", ""heel"", ""sofa"", ""integrity"", ""cereal"", ""tempt"", ""agree"", ""incongruous"", ""unpleasant"", ""leak"", ""floor"", ""border"", ""star"", ""drift"", ""mature"", ""government"", ""result"", ""automatic"", ""latest"", ""tropical"", ""convention"", ""infect"", ""ruin"", ""distort"", ""oral"", ""general"", ""presidential"", ""respect"", ""trail"", ""orientation"", ""glare"", ""guard"", ""relax"", ""revise"", ""qualification"", ""bush"", ""election"", ""output"", ""fibre"", ""activity"", ""soil"", ""expect"", ""storm"", ""inspector"", ""ditch"", ""revolution"", ""factor"", ""tourist"", ""reliable"", ""invite"", ""attachment"", ""carpet"", ""flower"", ""prey"", ""slice"", ""knife"", ""object"", ""insistence"", ""privilege"", ""redeem"", ""reality"", ""poem"", ""spell"", ""hospital"", ""jungle"", ""closed"", ""remark"", ""producer"", ""complication"", ""brainstorm"", ""acid"", ""mean"", ""day"", ""applaud"", ""firefighter"", ""conservation"", ""leader"", ""protect"", ""magnitude"", ""fail"", ""convenience"", ""thread"", ""needle"", ""privacy"", ""move"", ""abortion"", ""high"", ""bomber"", ""content"", ""distribute"", ""sweet"", ""mist"", ""falsify"", ""force"", ""agony"", ""edge"", ""contain"", ""pollution"", ""precede"", ""spine"", ""terrace"", ""bean"", ""cheque"", ""soap"", ""ideal"", ""get"", ""jest"", ""jurisdiction"", ""suitcase"", ""charm"", ""press"", ""fit"", ""painter"", ""dress"", ""chew"", ""volcano"", ""crouch"", ""percent"", ""fresh"", ""technique"", ""daughter"", ""whisper"", ""residence"", ""reject"", ""illness"", ""transform"", ""fiction"", ""dairy"", ""cabin"", ""thinker"", ""similar"", ""fisherman"", ""disorder"", ""well"", ""tender"", ""glove"", ""nervous"", ""stomach"", ""critic"", ""pay"", ""tragedy"", ""asset"", ""electron"", ""evolution"", ""angel"", ""nonsense"", ""plot"", ""likely"", ""predator"", ""movie"", ""disk"", ""abridge"", ""unfair"", ""cabinet"", ""favour"", ""infrastructure"", ""slab"", ""deal"", ""legislation"", ""objective"", ""turn"", ""musical"", ""thoughtful"", ""kinship"", ""initial"", ""genuine"", ""detector"", ""silk"", ""long"", ""overall"", ""overcharge"", ""demand"", ""depression"", ""hover"", ""gallon"", ""risk"", ""stun"", ""cream"", ""hiccup"", ""swear"", ""facade"", ""break in"", ""overview"", ""parameter"", ""theory"", ""strategic"", ""chase"", ""beginning"", ""substitute"", ""sun"", ""version"", ""speaker"", ""imposter"", ""turkey"", ""peanut"", ""resort"", ""wreck"", ""beautiful"", ""feminist"", ""suffer"", ""joke"", ""flour"", ""deer"", ""hostile"", ""sword"", ""murder"", ""definite"", ""health"", ""specimen"", ""pluck"", ""fuss"", ""fate"", ""official"", ""restrain"", ""serious"", ""threat"", ""platform"", ""barrier"", ""launch"", ""hospitality"", ""cap"", ""bow"", ""viable"", ""statement"", ""hell"", ""creep"", ""knowledge"", ""deport"", ""trouble"", ""dawn"", ""measure"", ""radio"", ""motorist"", ""staff"", ""constant"", ""behave"", ""relieve"", ""sensitivity"", ""crossing"", ""knit"", ""civilian"", ""arise"", ""menu"", ""bronze"", ""strikebreaker"", ""color-blind"", ""hand"", ""gravel"", ""society"", ""tube"", ""swim"", ""ample"", ""eyebrow"", ""taxi"", ""association"", ""list""";
        public static string GetPayload(string inputText, string inputWords)
        {
            string text = inputElement.Replace("#REPLACE ME#", inputText);
            string words = text.Replace("#INSERT WORDS#", inputWords);
            return inputCheckTest.Replace("#REPLACE ME#", words);
        }
        public static string GetOutput(string words, string numbers)
        {
            string[] nameReplace = words.Split(", ");
            string[] matchReplace = numbers.Split(", ");
            string data = "";
            for (int i = 0; i < nameReplace.Length; i++)
            {
                string temp = outputValue.Replace("#REPLACE ME#", nameReplace[i]);
                temp = temp.Replace("#NUMBER#", matchReplace[i]);
                data += temp + ",";
            }
            data = data.Remove(data.Length - 1);
            string oneEntity = outputElement.Replace("#REPLACE ME#", data);
            return outputCheckTest.Replace("#REPLACE ME#", oneEntity);
        }
    }
}
