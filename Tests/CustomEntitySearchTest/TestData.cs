using AzureCognitiveSearch.PowerSkills.Common;
using AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntitySearchTest
{
    public class TestData
    {
        public const string missingWordsBadRequestInput = @"{
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
        public const string missingTextBadRequestInput = @"{
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
        public const string emptyTextWordsNotFoundInput = @"""will you search?""";
        public const string emptyWordsEmptyEntitiesInput = @"""if you find when searching, i will be sad""";
        public const string largeTextQuickResultInputWords = @"""random"", ""drefke"", ""customLookup"", ""eyisi""";
        public const string largeWordsQuickResultInputText = @"""Azure Storage is a Microsoft-managed service providing cloud storage that is highly available," + 
        "secure, durable, scalable, and redundant. Azure Storage includes Azure Blobs (objects), Azure Data Lake Storage Gen2, Azure Files, " +
        @"Azure Queues, and Azure Tables. Learn how to leverage Azure Storage in your applications with our quickstarts and tutorials.""";
        public const string largeWordsQuickResultInputWords = @"""check1"", ""AzureStorageinyourapplicationswithourquickstartsandtutorials"", ""queues""";
        public const string largeWordsOutputNames = @"""Queues""";
        public const string largeWordsOutputMatchIndex = @"231";
        public const string largeWordsOutputFound = @"""queues""";
        public const string largeTextOutputNames = @"""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""Random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""random"", ""eyisi""";
        public const string largeTextOutputMatchIndex = @"77, 106, 158, 265, 313, 392, 632, 1158, 1447, 1564, 1725, 2026, 2254, 2414, 2732, 5657, 6665, 7316, 8219, 8255, 8477, 8538, 8782, 11759";
        public const string largeTextOutputFound = @"""random"",""eyisi""";
        public const string largeNumWordsOutputNames = @"""book"", ""book"", ""book"", ""test"", ""test"", ""page"", ""manual"", ""young"", ""paper"", ""paper"", ""word"", ""walk"", ""look"", ""surprise"", ""sip"", ""challenge"", ""doll"", ""reaction"", ""fish"", ""need"", ""need"", ""need"", ""need"", ""exercise"", ""letter"", ""stay"", ""promotion"", ""writer"", ""writer"", ""writer"", ""writer"", ""writer"", ""enjoy"", ""see"", ""see"", ""see"", ""see"", ""see"", ""see"", ""see"", ""old"", ""fine"", ""post"", ""block"", ""block"", ""block"", ""real"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""sentence"", ""project"", ""project"", ""project"", ""cut"", ""memory"", ""bird"", ""try"", ""try"", ""buy"", ""buy"", ""break"", ""possible"", ""red"", ""party"", ""home"", ""product"", ""hear"", ""proud"", ""brown"", ""night"", ""begin"", ""begin"", ""begin"", ""ice cream"", ""house"", ""fence"", ""Rock"", ""piece"", ""pot"", ""useful"", ""information"", ""allow"", ""ice"", ""common"", ""subject"", ""dark"", ""glass"", ""frame"", ""time"", ""time"", ""time"", ""game"", ""method"", ""method"", ""course"", ""floor"", ""automatic"", ""ruin"", ""brainstorm"", ""day"", ""day"", ""day"", ""move"", ""high"", ""high"", ""get"", ""get"", ""get"", ""get"", ""get"", ""technique"", ""similar"", ""likely"", ""movie"", ""long"", ""long"", ""cream"", ""hand"", ""hand"", ""list"", ""list"", ""list""";
        public const string largeNumWordsOutputMatchIndex = "4140, 4236, 4985, 5831, 7204, 23, 8689, 3268, 5749, 8757, 113, 7744, 1871, 1981, 10817, 800, 3008, 2083, 4636, 297, 3502, 4374, 5181, 1391, 3951, 7279, 6360, 526, 613, 875, 1093, 8629, 9086, 1192, 2066, 2397, 2467, 4225, 4723, 6635, 5221, 6942, 2189, 1003, 1149, 1339, 7977, 84, 165, 399, 480, 557, 639, 694, 931, 1165, 1307, 1454, 1732, 1831, 2033, 2261, 2739, 7323, 8226, 8484, 8944, 9042, 1200, 1850, 2498, 8596, 5842, 4624, 2386, 4798, 5099, 6502, 1329, 2551, 5107, 5209, 7695, 1714, 7529, 7573, 6204, 3080, 709, 1418, 1536, 4052, 3401, 6648, 5319, 5163, 9657, 2541, 6564, 1178, 4052, 672, 1114, 4949, 4926, 7815, 2971, 4731, 6011, 12232, 8522, 8874, 2363, 7856, 8892, 5036, 8290, 1505, 5051, 6760, 2133, 5344, 7605, 422, 1292, 1403, 4704, 5070, 8603, 8858, 35, 7419, 3124, 3946, 4056, 6294, 8403, 249, 5649, 8415";
        public const string largeNumWordsOutputFound = @"""book"",""test"",""page"",""manual"",""young"",""paper"",""word"",""walk"",""look"",""surprise"",""sip"",""challenge"",""doll"",""reaction"",""fish"",""need"",""exercise"",""letter"",""stay"",""promotion"",""writer"",""enjoy"",""see"",""old"",""fine"",""post"",""block"",""real"",""sentence"",""project"",""cut"",""memory"",""bird"",""try"",""buy"",""break"",""possible"",""red"",""party"",""home"",""product"",""hear"",""proud"",""brown"",""night"",""begin"",""ice cream"",""house"",""fence"",""rock"",""piece"",""pot"",""useful"",""information"",""allow"",""ice"",""common"",""subject"",""dark"",""glass"",""frame"",""time"",""game"",""method"",""course"",""floor"",""automatic"",""ruin"",""brainstorm"",""day"",""move"",""high"",""get"",""technique"",""similar"",""likely"",""movie"",""long"",""cream"",""hand"",""list""";

        public const string missingWordsExpectedResponse = @" - Error processing the request record : System.Collections.Generic.KeyNotFoundException: The given key 'words' was not present in the dictionary.
   at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
   at AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch.CustomEntitySearch.<>c__DisplayClass1_0.<Run>b__0(WebApiRequestRecord inRecord, WebApiResponseRecord outRecord) in C:\Users\t-neja\azure-search-power-skills\Text\CustomEntitySearch\CustomEntitySearch.cs:line 55
   at AzureCognitiveSearch.PowerSkills.Common.WebApiSkillHelpers.ProcessRequestRecords(String functionName, IEnumerable`1 requestRecords, Func`3 processRecord) in C:\Users\t-neja\azure-search-power-skills\WebAPISkillHelper.cs:line 33";
        public const string missingTextExpectedResponse = @" - Error processing the request record : System.Collections.Generic.KeyNotFoundException: The given key 'text' was not present in the dictionary.
   at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
   at AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch.CustomEntitySearch.<>c__DisplayClass1_0.<Run>b__0(WebApiRequestRecord inRecord, WebApiResponseRecord outRecord) in C:\Users\t-neja\azure-search-power-skills\Text\CustomEntitySearch\CustomEntitySearch.cs:line 54
   at AzureCognitiveSearch.PowerSkills.Common.WebApiSkillHelpers.ProcessRequestRecords(String functionName, IEnumerable`1 requestRecords, Func`3 processRecord) in C:\Users\t-neja\azure-search-power-skills\WebAPISkillHelper.cs:line 33";
        public const string outputCheckTest = @"{""Values"":[#REPLACE ME#]}";
        public const string outputElement = @"{""RecordId"":""1"",""Data"":{""Entities"":[#REPLACE ME#],""EntitiesFound"":[#INSERT WORDS#]},""Errors"":[],""Warnings"":[]}";
        public const string outputValue = @"{""Name"":#REPLACE ME#,""MatchIndex"":#NUMBER#}";
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
        public static string GetPayload(string inputText, string inputWords, int numDocs = 0)
        {
            string addTextToElement = inputElement.Replace("#REPLACE ME#", inputText);
            string addWordsToElement = addTextToElement.Replace("#INSERT WORDS#", inputWords);
            if (numDocs <= 0)
            {
                return inputCheckTest.Replace("#REPLACE ME#", addWordsToElement);
            }
            else
            {
                string entitiesFound = String.Concat(Enumerable.Repeat(addWordsToElement + ",", numDocs));
                entitiesFound = entitiesFound.Remove(entitiesFound.Length - 1);
                return inputCheckTest.Replace("#REPLACE ME#", entitiesFound);
            }
        }
        public static string GetOutput(string nameEntities, string matchEntities, string foundEntities, int numDocs = 0)
        {
            string[] nameReplace = nameEntities.Split(", ", StringSplitOptions.RemoveEmptyEntries);
            string[] matchReplace = matchEntities.Split(", ", StringSplitOptions.RemoveEmptyEntries);
            string entities = "";
            for (int i = 0; i < nameReplace.Length; i++)
            {
                string temp = outputValue.Replace("#REPLACE ME#", nameReplace[i]);
                temp = temp.Replace("#NUMBER#", matchReplace[i]);
                entities += temp + ",";
            }

            if (nameReplace.Length > 0)
            {
                entities = entities.Remove(entities.Length - 1);
            }
            
            
            if (numDocs <= 0)
            {
                string oneEntity = outputElement.Replace("#REPLACE ME#", entities).Replace("#INSERT WORDS#", foundEntities);
                return outputCheckTest.Replace("#REPLACE ME#", oneEntity);
            }
            else
            {
                string allInputEntityElements = "";
                for (int j = 0; j < TestData.numDocs; j++)
                {
                    allInputEntityElements += TestData.outputElement.Replace("#REPLACE ME#", entities).Replace("#INSERT WORDS#", foundEntities) + ",";
                }
                allInputEntityElements = allInputEntityElements.Remove(allInputEntityElements.Length - 1);
                return outputCheckTest.Replace("#REPLACE ME#", allInputEntityElements);
            }
        }

        public static async Task<WebApiSkillResponse> GeneratePayloadRequest(String inputText)
        {
            var jsonContent = new StringContent(inputText, null, "application/json");
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                ContentType = "application/json; charset=utf-8",
                Body = await jsonContent.ReadAsStreamAsync(),
                Method = "POST"
            };
            var response = (OkObjectResult)(await CustomEntitySearch.Run(
                request, new LoggerFactory().CreateLogger("local"), new Microsoft.Azure.WebJobs.ExecutionContext()));
            return (WebApiSkillResponse)response.Value;
        }
    }
}
