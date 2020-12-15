using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Lab7
{
    class Program
    {
        delegate List<T> GetList<T>();

        static void Main(string[] args)
        {
            while (true)
            {
                switch (MainMenu())
                {
                    case 1:
                        ShowCards();
                        break;
                    case 2:
                        AddUser();
                        break;
                    case 3:
                        AddPoints();
                        break;
                    case 4:
                        SpendPoints();
                        break;
                }
            }
            
        }

        private static void SpendPoints()
        {
            var cards = GetCards();
            Console.WriteLine("Enter card id");
            int cardId = Convert.ToInt32(Console.ReadLine());

            var card = cards.FirstOrDefault(it => it.Id == cardId);
            if (card == null)
            {
                Console.WriteLine("Invalid id");
                return;
            }

            Console.WriteLine("Enter points to spend");
            int points = Convert.ToInt32(Console.ReadLine());

            cards.ForEach(it => {
                if (it.Id == cardId)
                {
                    it.Amount -= points;
                }
            });

            File.WriteAllText("cards.xml", cards.FromBonusCards());
        }

        private static void AddPoints()
        {
            var cards = GetCards();
            Console.WriteLine("Enter card id");
            int cardId = Convert.ToInt32(Console.ReadLine());

            var card = cards.FirstOrDefault(it => it.Id == cardId);
            if (card == null)
            {
                Console.WriteLine("Invalid id");
                return;
            }

            Console.WriteLine("Enter points to add");
            int points = Convert.ToInt32(Console.ReadLine());

            cards.ForEach(it => {
                if (it.Id == cardId)
                {
                    it.Amount += points;
                }
            });

            File.WriteAllText("cards.xml", cards.FromBonusCards());
        }

        private static void AddUser()
        {
            User user;
            Console.WriteLine("Enter name");
            var name = Console.ReadLine();
            Console.WriteLine("Do you have card?");
            var isWithCard = Console.ReadLine().Equals("yes");

            GetList<User> getList = GetUsers;
            var users = getList();
            var lastUserId = users[users.Count - 1].Id;

            GetList<BonusCard> getListCards = GetCards;
            var cards = getListCards();
            var lastCardId = cards[cards.Count - 1].Id;
            

            if (isWithCard)
            {
                user = new UserWithCard
                {
                    Id = lastUserId + 1,
                    Name = name,
                    CardId = lastCardId + 1
                };
                BonusCard card = new BonusCard
                {
                    Id = lastCardId + 1,
                    Amount = 0
                };
                cards.Add(card);
                File.WriteAllText("cards.xml", cards.FromBonusCards());
            }
            else
            {
                user = new UserWithoutCard
                {
                    Id = lastUserId + 1,
                    Name = name
                };
            }

            users.Add(user);
            File.WriteAllText("users.xml", users.FromUsers());
        }

        static List<User> GetUsers() {
            var xDocument = XDocument.Load("users.xml");
            var query = from element in xDocument.Descendants("user") select element;
            var users = new List<User>();
            foreach (XElement item in query)
            {
                users.Add(item.ToUser());
            }
            return users;
        }

        static List<BonusCard> GetCards()
        {
            var xDocument = XDocument.Load("cards.xml");
            var query = from element in xDocument.Descendants("card") select element;
            var cards = new List<BonusCard>();
            foreach (XElement item in query)
            {
                cards.Add(item.ToBonusCard());
            }
            return cards;
        }

        static int MainMenu()
        {
            Console.WriteLine("1 - Show cards");
            Console.WriteLine("2 - Add users");
            Console.WriteLine("3 - Add points");
            Console.WriteLine("4 - Spend points");
            var result = Convert.ToInt32(Console.ReadLine());
            return result;
        }

        static void ShowCards()
        {
            var cards = GetCards();
            Console.WriteLine("ID Amount");
            cards.ForEach(it =>
            {
                Console.WriteLine(it.Id + " " + it.Amount);
            });
        }

        static void CreateCards()
        {
            var cards = new List<BonusCard>
                    {
                        new BonusCard
                        {
                            Id = 1,
                            Amount = 100
                        },
                        new BonusCard
                        {
                            Id = 2,
                            Amount = 200
                        },
                        new BonusCard
                        {
                            Id = 1,
                            Amount = 300
                        }
                    };
            File.WriteAllText("cards.xml", cards.FromBonusCards());
        }
    }

    class BonusCard
    {
        public int Id { get; set; }
        public int Amount { get; set; }

        public XElement ToXElement()
        {
            return new XElement("card", new XAttribute("id", Id), new XAttribute("Amount", Amount));
        }
    }

    partial class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    abstract partial class User
    {
        public abstract XElement ToXElement();
    }

    class UserWithCard : User
    {
        public int CardId { get; set; }

        public override XElement ToXElement()
        {
            return new XElement("user", new XAttribute("id", Id), new XAttribute("name", Name), new XAttribute("cardId", CardId));
        }
    }

    class UserWithoutCard : User
    {
        public override XElement ToXElement()
        {
            return new XElement("user", new XAttribute("id", Id), new XAttribute("name", Name));
        }
    }

    static class Extensions
    {
        public static BonusCard ToBonusCard(this XElement element)
        {
            return new BonusCard
            {
                Id = Convert.ToInt32(element.Attribute("id").Value),
                Amount = Convert.ToInt32(element.Attribute("Amount").Value)
            };
        }

        public static string FromBonusCards(this List<BonusCard> cards)
        {
           return new XElement("cards", cards.Select(it => it.ToXElement())).ToString();
        }

        public static User ToUser(this XElement element)
        {
            if (element.Attribute("cardId") == null)
            {
                return new UserWithoutCard
                {
                    Id = Convert.ToInt32(element.Attribute("id").Value),
                    Name = element.Attribute("name").Value
                };
            } else
            {
                return new UserWithCard
                {
                    Id = Convert.ToInt32(element.Attribute("id").Value),
                    Name = element.Attribute("name").Value,
                    CardId = Convert.ToInt32(element.Attribute("cardId").Value)
                };
            }
        }

        public static string FromUsers(this List<User> users)
        {
            return new XElement("users", users.Select(it => it.ToXElement())).ToString();
        }
    }
}
