// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntityLookupTests
{
    [TestClass]
    [TestCategory("CustomEntityLookup")]
    public class MatchDistanceMisc : MatchValidationBase
    {
        [TestMethod]
        public void HandlePunctuationInText()
        {
            TestFindMatch(
                text: "Once... Upon... A Time...",
                words: "Once",
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0);

            TestFindMatch(
                text: "Once... Upon... A Time...",
                words: "Once Upon",
                expectedMatches: 0,
                allowableFuziness: 0,
                expectedFuziness: null);
        }

        [TestMethod]
        public void HandlePunctuationInWord()
        {
            TestFindMatch(
                text: "Once... Upon... A Time...",
                words: ".......Upon.......",
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0);
        }

        [TestMethod]
        public void ReplaceOverAddAndRemoves()
        {
            TestFindMatch(
                text: "ztbcdefghijclmnoxz",
                words: "zqbcdefghijclmnoyz",
                expectedMatches: 1,
                allowableFuziness: 2,
                expectedFuziness: 2);
        }

        [TestMethod]
        public void MatchBeginningOfText()
        {
            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "the",
                expectedMatches: 2,
                allowableFuziness: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: new string[] { "  Te  ", "Teh", "Thex" },
                expectedMatches: 3,
                allowableFuziness: 1,
                expectedFuziness: 1);
        }

        [TestMethod]
        public void MatchEndOfText()
        {
            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "rive",
                expectedMatches: 1,
                allowableFuziness: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: new string[] { "rive  ",  "iver", "riverr"},
                expectedMatches: 4,
                allowableFuziness: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "riverr  ",
                expectedMatches: 1,
                allowableFuziness: 1,
                expectedFuziness: 1);
        }

        [TestMethod]
        public void UnfoundWord()
        {
            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "Zombies",
                expectedMatches: 0,
                allowableFuziness: 3);
        }

        [TestMethod]
        public void TestFindFuzzyMatchesMidSentence()
        {
            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "brwn",
                expectedMatches: 1,
                allowableFuziness: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "brozwn",
                expectedMatches: 1,
                allowableFuziness: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "zbrown",
                expectedMatches: 1,
                allowableFuziness: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "brownz",
                expectedMatches: 1,
                allowableFuziness: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "brzwn",
                expectedMatches: 1,
                allowableFuziness: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "brzwn",
                expectedMatches: 1,
                allowableFuziness: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "The brown fox jumped over the river",
                words: "brownfox",
                expectedMatches: 0,
                allowableFuziness: 1,
                expectedFuziness: 1);
        }


        [TestMethod]
        public void CanFindMultipleMatches()
        {
            TestFindMatch(
                text: "!hello!hello!hello!",
                words: "hello",
                expectedMatches: 3,
                allowableFuziness: 0,
                expectedFuziness: 0);
        }

        [TestMethod]
        public void LargeFuzinessBroadlyMatch()
        {
            TestFindMatch(
                text: "Once upon a time in a land far away there lived a dragon that sang with fire!",
                words: "aeiou", // vowels
                expectedMatches: 9,
                allowableFuziness: 5,
                expectedFuziness: 4,
                caseSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInGreek()
        {
            TestFindMatch(
                text: "Tου Αντώνη Ρέλλα - Θα πρέπει να γίνει κατανοητό ότι οι αποκλεισμοί των αναπήρων εκκινούν, έτσι κι αλλιώς, από τις θεσμοθετημένες πρακτικές του κράτους και τα εμπόδια στο δομημένο περιβάλλον. Πώς, λοιπόν, η κυβέρνηση θα κάνει πράξη την ανεξάρτητη διαβίωση",
                words: new string[] { "έτσι", "έτσι" },
                expectedMatches: 2,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInThai()
        {
            TestFindMatch(
                text: "เพื่อนสนิท ยังบอกอีกว่า ก่อนจะเสียชีวิต ภรรยาของครูประสิทธิ์ ได้เปิดไลน์ส่วนตัวที่ครูประสิทธิ์ ส่งถึงภรรยา มาให้ตนอ่าน พบข้อความว่า ครูประสิทธิ์ไลน์มาขอกินข้าวกับภรรยาเป็นมื้อสุดท้าย เพราะไม่รู้ว่าจะอยู่ต่อไปได้อีกกี่วัน ซึ่งภรรยาก็บอกว่า ให้กลับมาทานข้าวด้วยกันที่บ้าน แต่ยังไม่ทันได้กลับบ้าน ตำรวจโทรมาบอกว่า พบศพครูประสิทธิ์ตายในรีสอร์ต ภรรยา และลูกๆ ของครู จึงเดินทางไปดู ก็พบว่าเสียชีวิตพร้อมเด็ก 14 ปี ภรรยาและลูกๆ ไม่มีใครพูดอะไร ก่อนจะดำเนินการขอรับศพครู กลับมาที่บ้าน เพื่อประกอบพิธีทางศาสนาและจะมีการฌาปนกิจศพในวันเสาร์ที่จะถึงนี้",
                words: new string[] { "ครูประสิทธิ์ไลน์มาขอกินข้าวกับภรรยาเป็นมื้อสุดท้าย", "ครูประสิทธิ์ไลน์มาขอกินข้าวกับภรรยาเป็นมื้อสุดทาย" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInHebrew()
        {
            TestFindMatch(
                text: "מורה לספרות נחשדת בשמאלנות בעיצומה של מלחמת צוק איתן. זהו הנושא הרשמי של בשבח המלחמה. אבל אין זה םפר של תקופה אלא םפר של אמן, סטייליסט מלא תנופה ותעופה. ספרו מחליק בצד התקופה וחותר יותר אל שורשם האפל של הדברים",
                words: new string[] { "םפר", "ם֫פר" },
                expectedMatches: 2,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInTurkish()
        {
            TestFindMatch(
                text: "Brezilya Serie A ekiplerinden Palmeiras, Beşiktaş'ın da transfer gündeminde yer alan Vitor Hugo'yu kadrosuna kattığını açıkladı",
                words: new string[] { "Vitor", "Vîtor" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInCzech()
        {
            TestFindMatch(
                text: "Po vyplnění kontaktního formuláře na e-shopu přijde jen automatická odpověď. Žena na zákaznické lince řekne, že s prodejcem nemá nic společného a že infolinka je i pro další e-shopy se „zázračnými“ léky. Pak přepojí na reklamační oddělení, z něhož se vyklube jen další automat.",
                words: new string[] { "Žena", "Zena" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInHungarian()
        {
            TestFindMatch(
                text: "Az Aquamant megformáló hollywoodi színész, aki a nyáron több hétig Magyarországon forgatott, egyik legkedvesebb kollégájával találkozott.",
                words: new string[] { "Magyarországon", "Magyarórszágon" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInArabic()
        {
            TestFindMatch(
                text: @"الأفغانية كابول، ومنطقة بورنو بشمال شرق نيجيريا، وأدت لسقوط عشرات القتلى والجرحى.

وقدم المصدر العزاء والمواساة لذوي الضحايا ولحكومتي وشعبي جمهورية أفغانستان الإسلامية وجمهورية نيجيريا الاتحادية",
                words: new string[] { "العزاء", "العٌزاء" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInJapanese()
        {
            TestFindMatch(
                text: "阪急電鉄と阪神電鉄は大阪の玄関口である「梅田駅」を「大阪梅田駅」にそれぞれ変更することを決めた。関係者によると、変更は１０月１日から。外国人観光客の利用が増える中、駅が大阪市の中心部にあることをわかりやすくすることが狙いだという。阪急電鉄は同じ狙いで、京都市中心部の河原町駅についても１０月から「京都河原町駅」に変更する。",
                words: new string[] { "外国人観光客の利用が増える中", "外国人観光客の利用が増える中" },
                expectedMatches: 2,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInFinish()
        {
            TestFindMatch(
                text: "Kuusi vuotta taksia ajanut Mika Lindberg ei enää aja mielellään Helsinki-Vantaan lentoasemalle. ”Kuskit kiukuttelevat siellä”, hän sanoo. Kiukuttelun syiksi Lindberg mainitsee lentokentän liikennettä sekoittavan terminaalityömaan ja koko taksialaa hämmentäneen taksiuudistuksen.",
                words: new string[] { "enää", "enäa" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInDanish()
        {
            TestFindMatch(
                text: "Cubanere kan fra i dag tilgå internettet lovligt fra deres nye hjem. Sådan lyder det i en lov, der blev vedtaget i maj, og netop er trådt i kraft.",
                words: new string[] { "hjem", "hjæm" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInNorwegian()
        {
            TestFindMatch(
                text: "I en uttalelse melder militæret at fem av de omkomne i styrten i landsbyen Mora Kalu utenfor Rawalpindi var soldater. Flyets to piloter er også bekreftet omkommet.",
                words: new string[] { "omkomne", "omkone" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInKorean()
        {
            TestFindMatch(
                text: @"왜 그리 내게 차가운가요
                            사랑이 그렇게 쉽게
                            변하는 거였나요
                            내가 뭔가 잘못했나요
                            그랬다면 미안합니다",
                words: new string[] { "잘못했나요", "잘못했나ㅇ" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInPolish()
        {
            TestFindMatch(
                text: "Na przełomie września i października 2017 roku w większości krajów Europy - m.in. w Niemczech, Austrii, Włoszech, Szwajcarii, Francji, Grecji, Norwegii, Rumunii, Bułgarii, a także w Polsce - zanotowano w powietrzu śladowe ilości radioaktywnego rutenu-106.",
                words: new string[] { "przełomie", "przelomie" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInRussian()
        {
            TestFindMatch(
                text: "Неадекватный поклонник разгромил машину культовой рок-исполнительницы Земфиры в центре Москвы. Вандал обрушился на «Мерседес» артистки, разбил стёкла и значительно повредил кузов авто. Безумец пояснил правоохранителям",
                words: new string[] { "в", "вы" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInSwedish()
        {
            TestFindMatch(
                text: "När den amerikanske rapartisten ASAP Rocky frihetsberövas i Stockholm, misstänkt för misshandel, väcker det starka reaktioner i USA.",
                words: new string[] { "misshandel", "misshändel" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInItalian()
        {
            TestFindMatch(
                text: "Nel governo la temperatura sale e non per il caldo. Nei rapporti tra Lega e 5Stelle - chiusa la finestra del voto a settembre - i rapporti sono diventati roventi.",
                words: new string[] { "roventi", "róventi" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInPortugese()
        {
            TestFindMatch(
                text: "Discute com ex-patrão por salário de mil euros em atraso e acaba morto à pancada",
                words: new string[] { "salário", "salario" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInFrench()
        {
            TestFindMatch(
                text: "Le corps retrouvé lundi dans la Loire est «très probablement» celui du jeune Steve Maia Caniço, a indiqué à l'AFP une source proche du dossier. Cécile de Oliveira, avocate de la famille du jeune homme, a également indiqué qu'il s'agit «probablement» du corps de Steve sur BFMTV. Une autre source proche du dossier a affirmé dans la soirée que l'autopsie aurait lieu mardi, «à 10h30».",
                words: new string[] { "dossier", "dossìer" },
                expectedMatches: 2,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInSpanish()
        {
            TestFindMatch(
                text: "Dieciséis de los fallecidos en las cinco horas que duró el suceso fueron decapitados y el resto murió asfixiado por el humo. Los reclusos patearon las cabezas cortadas, grabaron las imágenes y las difundieron por WhatsApp, según informa el digital Ponte. Las autoridades han detallado que dos funcionarios de prisiones fueron hechos rehenes, pero ya han sido liberados tras las negociaciones de las autoridades",
                words: new string[] { "liberados", "liberadós" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInDutch()
        {
            TestFindMatch(
                text: "Nog enkele dagen en het veelbesproken boerkaverbod gaat in. Nikabdraagsters roepen om het hardst dat de overheid hun vrijheid aantast. De Rotterdamse Jamila (37) maakte echter kennis met de onvrijwillige kant van de sluier. In Pakistan dwong haar schoonfamilie haar om een boerka te dragen. Terug in Nederland wierp ze het ding af. Het stuk stof is niet het grootste probleem, vindt ze. ",
                words: new string[] { "aantast", "antast" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void TestCanFindInGerman()
        {
            TestFindMatch(
                text: "üngstes Beispiel ist die Festsetzung eines russischen Tankers im Gebiet Odessa. Das Schiff war nach ukrainischen Angaben im November an Russlands Blockade der Meerenge von Kertsch beteiligt, bei der drei ukrainische Marineschiffe aufgebracht wurden. Die 24 Ukrainer auf den drei Schiffen sind, trotz einer Anordnung des Internationalen Seegerichtshofs von Ende Mai, weiter in russischer Untersuchungshaft; Selenskyj will ihre Freilassung erreichen.",
                words: new string[] { "Untersuchungshaft", "Untersuchüngshaft" },
                expectedMatches: 1,
                allowableFuziness: 0,
                expectedFuziness: 0,
                caseSensitive: false,
                accentSensitive: false);
        }

        [TestMethod]
        public void FindDuplicateMatches()
        {
            TestFindMatch(
                text: "hello!hello!hello",
                words: new string[] { "hello", "hello" },
                expectedMatches: 6, //  note each word is found twice (once for each entity)
                allowableFuziness: 0,
                expectedFuziness: 0);
        }

        [TestMethod]
        public void TestMultipleAccents()
        {
            TestFindMatch(
                text: "àà",
                words: new string[] { "aa" },
                expectedMatches: 1, 
                allowableFuziness: 1,
                expectedFuziness: 1);
        }
    }
}
