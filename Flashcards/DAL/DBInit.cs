using Flashcards.Models;
using Microsoft.EntityFrameworkCore;

namespace Flashcards.DAL;

public static class DbInit
{
    public static void Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<FlashcardsDbContext>();

        // Apply pending migrations (Creates database if it doesn't exist)
        context.Database.Migrate();

        // Populates Subjects table if empty
        if (!context.Subjects.Any())
        {
            // Create a list of Subjects
            var subjects = new List<Subject>
            {
                new Subject { Name = "Software Testing", Description = "This subject was generated using GPT-3.5", Visibility = SubjectVisibility.Public },
                new Subject { Name = "History", Visibility = SubjectVisibility.Public },
                new Subject { Name = "Science", Visibility = SubjectVisibility.Public },
                new Subject { Name = "German", Visibility = SubjectVisibility.Public },
                new Subject { Name = "Norwegian", Visibility = SubjectVisibility.Public },
            };

            context.AddRange(subjects);
            context.SaveChanges();
        }

        // Populates Decks table if empty
        if (!context.Decks.Any())
        {
            // Create a list of Decks
            var decks = new List<Deck>
            {
                // Testing
                new Deck { Name = "Basics of Software Testing", Description = "This deck was generated using GPT-3.5", SubjectId = 1 },
                new Deck { Name = "Testing Methodologies", Description = "This deck was generated using GPT-3.5", SubjectId = 1 },
                
                // HistoryGene
                new Deck { Name = "World War II", SubjectId = 2 },
                
                // Physics
                new Deck { Name = "Physics", SubjectId = 3 },
                
                // German
                new Deck { Name = "Common Phrases", SubjectId = 4 },

                // Norwegian
                new Deck { Name = "Days", SubjectId = 5 },
                new Deck { Name = "Adjectives", SubjectId = 5 },
                new Deck { Name = "Colors", SubjectId = 5 },
                new Deck { Name = "Countries in Europe", SubjectId = 5},
            };

            context.AddRange(decks);
            context.SaveChanges();
        }

        // Populates Cards table if empty
        if (!context.Cards.Any())
        {
            // Create a list of Cards
            var cards = new List<Card>
            {
                // Testing
                new Card { Front = "What is software testing?", Back = "Software testing is the process of evaluating a software application to identify and fix any defects or issues.", DeckId = 1 },
                new Card { Front = "What is the purpose of software testing?", Back = "To ensure that the software functions correctly and meets the specified requirements.", DeckId = 1 },
                new Card { Front = "What is a test case?", Back = "A test case is a set of conditions or actions used to determine whether a software application works as expected.", DeckId = 1 },
                new Card { Front = "What is regression testing?", Back = "The process of retesting software to ensure that changes haven't introduced defects.", DeckId = 1 },
                new Card { Front = "Define functional testing.", Back = "Functional testing checks if the software functions according to the specified requirements.", DeckId = 1 },
                new Card { Front = "What is load testing?", Back = "Load testing assesses how well a system can handle a specific load or level of user activity.", DeckId = 1 },
                new Card { Front = "What is automated testing?", Back = "A testing approach that uses automation tools to execute test cases and compare actual results with expected results.", DeckId = 2 },
                new Card { Front = "What is manual testing?", Back = "Manual testing is a testing approach where test cases are executed by human testers without the use of automation tools.", DeckId = 2 },
                new Card { Front = "Explain the concept of black-box testing.", Back = "Black-box testing focuses on testing the functionality of a software application without knowledge of its internal code.", DeckId = 2 },
                new Card { Front = "What is the purpose of white-box testing?", Back = "Evaluating the internal logic and structure of a software application, focusing on code-level testing.", DeckId = 2 },
                new Card { Front = "What is usability testing?", Back = "Usability testing assesses how user-friendly and intuitive a software application is for its intended users.", DeckId = 2 },
                new Card { Front = "Define smoke testing.", Back = "Smoke testing is an initial test to ensure that the most critical functions of the software are working properly.", DeckId = 2 },
                new Card { Front = "What is boundary testing?", Back = "Boundary testing examines how the software behaves at the edges or boundaries of valid input values.", DeckId = 2 },
                
                // History
                new Card { Front = "What year did World War II start?", Back = "1939", DeckId = 3 },
                new Card { Front = "What year did World War II end?", Back = "1945", DeckId = 3 },
                new Card { Front = "What was the code name for the Battle of Normandy?", Back = "Operation Overlord", DeckId = 3 },
                new Card { Front = "What research and development project produced the first nuclear weapons during World War II?", Back = "Manhattan Project", DeckId = 3 },
                new Card { Front = "What was the second city the United States dropped a nuclear bomb on?", Back = "Nagasaki", DeckId = 3 },
                new Card { Front = "On which beach did the Americans run into a firestorm of resistance during the D-Day landings?", Back = "Omaha", DeckId = 3 },
                new Card { Front = "Where was the first atom bomb tested?", Back = "New Mexico", DeckId = 3 },
                new Card { Front = "What was the Nazi code name for a planned invasion of the United Kingdom during WW2?", Back = "Operation Sea Lion", DeckId = 3 },
                new Card { Front = "What was the largest battleship of World War 2?", Back = "Yamato", DeckId = 3 },
                new Card { Front = "In May 1941, Vidkund Quisling became Head of State in which country?", Back = "Norway", DeckId = 3 },
                
                // Physics
                new Card { Front = "What is the speed of light?", Back = "299,792,458 m/s", DeckId = 4 },
                new Card { Front = "What is the acceleration due to gravity?", Back = "9.81 m/s^2", DeckId = 4 },
                new Card { Front = "Where does sound travel faster; water or air?", Back = "Water", DeckId = 4 },
                new Card { Front = "What is opposite to matter?", Back = "Antimatter", DeckId = 4 },
                new Card { Front = "What is the Law of Conservation of Energy?", Back = "The energy of the Universe is constant; it can neither be created or destroyed but only transferred and transformed.", DeckId = 4 },
                new Card { Front = "What is the name of the layer of air closest to us in the astmosphere?", Back = "Troposphere", DeckId = 4 },
                new Card { Front = "How many volts can an electric eel produce?", Back = "Around 500 volts.", DeckId = 4 },
                new Card { Front = "What unit is used to measure the intensity of light?", Back = "Candela (cd)", DeckId = 4 },
                new Card { Front = "What is the hardest known substance?", Back = "Diamond", DeckId = 4 },
                new Card { Front = "Do you weigh less, the same, or more at the equator?", Back = "Less. Gravity is less at the equator due to the centrifugal force of the spinning earth.", DeckId = 4 },
                new Card { Front = "Which branch of physics is concerned with heat and temperature and their relation to energy and work?", Back = "Thermodynamics", DeckId = 4 },

                // German
                new Card { Front = "What's your name?", Back = "Wie heißt du?", DeckId = 5 },
                new Card { Front = "How are you?", Back = "Wie geht es dir?", DeckId = 5 },
                new Card { Front = "How old are you?", Back = "Wie alt bist du?", DeckId = 5 },
                new Card { Front = "Where do you come from?", Back = "Woher kommst du aus?", DeckId = 5 },
                new Card { Front = "I'm sorry", Back = "Es tut mir leid", DeckId = 5 },
                new Card { Front = "I don't understand what you're saying", Back = "Ich verstehe nicht, was Sie sagen", DeckId = 5 },
                new Card { Front = "Do you speak English?", Back = "Sprechen Sie Englisch?", DeckId = 5 },
                new Card { Front = "Can you help me?", Back = "Kannst du mir bitte helfen?", DeckId = 5 },
                new Card { Front = "How much does it cost?", Back = "Was kostet das?", DeckId = 5 },
                new Card { Front = "Thank you very much!", Back = "Vielen Dank!", DeckId = 5 },

                // Norwegian - Days
                new Card { Front = "Monday", Back = "Mandag", DeckId = 6},
                new Card { Front = "Tuesday", Back = "Tirsdag", DeckId = 6},
                new Card { Front = "Wednesday", Back = "Onsdag", DeckId = 6},
                new Card { Front = "Thursday", Back = "Torsdag", DeckId = 6},
                new Card { Front = "Friday", Back = "Fredag", DeckId = 6},
                new Card { Front = "Saturday", Back = "Lørdag", DeckId = 6},
                new Card { Front = "Sunday", Back = "Søndag", DeckId = 6},

                // Norwegian - Adjectives
                new Card { Front = "Cold", Back = "Kald", DeckId = 7},
                new Card { Front = "Warm", Back = "Varm", DeckId = 7},
                new Card { Front = "Slippery", Back = "Glatt", DeckId = 7},
                new Card { Front = "Wet", Back = "Våt", DeckId = 7},
                new Card { Front = "Dry", Back = "Tørr", DeckId = 7},
                new Card { Front = "Big", Back = "Stor", DeckId = 7},
                new Card { Front = "Small", Back = "Liten", DeckId = 7},
                new Card { Front = "Difficult", Back = "Vanskelig", DeckId = 7},
                new Card { Front = "Easy", Back = "Lett", DeckId = 7},
                new Card { Front = "Interesting", Back = "Interessant", DeckId = 7},
                new Card { Front = "Cool", Back = "Kul", DeckId = 7},
                new Card { Front = "Funny", Back = "Morsom", DeckId = 7},
                new Card { Front = "Magical", Back = "Magisk", DeckId = 7},
                new Card { Front = "New", Back = "Ny", DeckId = 7},
                new Card { Front = "Old", Back = "Gammel", DeckId = 7},

                // Norwegian - Colors
                new Card { Front = "Blue", Back = "Blå", DeckId = 8},
                new Card { Front = "Red", Back = "Rød", DeckId = 8},
                new Card { Front = "Yellow", Back = "Gul", DeckId = 8},
                new Card { Front = "Green", Back = "Grønn", DeckId = 8},
                new Card { Front = "Orange", Back = "Oransj", DeckId = 8},
                new Card { Front = "Purple", Back = "Lilla", DeckId = 8},
                new Card { Front = "Brown", Back = "Brun", DeckId = 8},
                new Card { Front = "White", Back = "Hvit", DeckId = 8},
                new Card { Front = "Grey", Back = "Grå", DeckId = 8},
                new Card { Front = "Black", Back = "Svart", DeckId = 8},
                new Card { Front = "Pink", Back = "Rosa", DeckId = 8},
                new Card { Front = "Teal", Back = "Blågrønn", DeckId = 8},
                new Card { Front = "Turquoise", Back = "Turkis", DeckId = 8},

                // Norwegian - Countries
                new Card { Front = "Norway", Back = "Norge", DeckId = 9},
                new Card { Front = "Sweden", Back = "Sverige", DeckId = 9},
                new Card { Front = "Denmark", Back = "Danmark", DeckId = 9},
                new Card { Front = "Finland", Back = "Finland", DeckId = 9},
                new Card { Front = "Iceland", Back = "Island", DeckId = 9},
                new Card { Front = "Germany", Back = "Tyskland", DeckId = 9},
                new Card { Front = "England", Back = "England", DeckId = 9},
                new Card { Front = "Great Britain", Back = "Storbritannia", DeckId = 9},
                new Card { Front = "Scottland", Back = "Skottland", DeckId = 9},
                new Card { Front = "Ireland", Back = "Irland", DeckId = 9},
                new Card { Front = "Belgium", Back = "Belgia", DeckId = 9},
                new Card { Front = "Netherlands", Back = "Nederland", DeckId = 9},
                new Card { Front = "Italy", Back = "Italia", DeckId = 9},
                new Card { Front = "Spain", Back = "Spania", DeckId = 9},
                new Card { Front = "Portugal", Back = "Portugal", DeckId = 9},
                new Card { Front = "France", Back = "Frankrike", DeckId = 9},
                new Card { Front = "Russia", Back = "Russland", DeckId = 9},
                new Card { Front = "Turkey", Back = "Tyrkia", DeckId = 9},
                new Card { Front = "Ukraine", Back = "Ukraina", DeckId = 9},
                new Card { Front = "Poland", Back = "Polen", DeckId = 9},
                new Card { Front = "Austria", Back = "Østerrike", DeckId = 9},
                new Card { Front = "Serbia", Back = "Serbia", DeckId = 9},
                new Card { Front = "Switzerland", Back = "Sveits", DeckId = 9},
                new Card { Front = "Greece", Back = "Hellas", DeckId = 9},
                new Card { Front = "Croatia", Back = "Kroatia", DeckId = 9},
                new Card { Front = "Romania", Back = "Romania", DeckId = 9},
                new Card { Front = "Hungary", Back = "Ungarn", DeckId = 9},

            };

            context.AddRange(cards);
            context.SaveChanges();
        }

        context.SaveChanges();
    }
}