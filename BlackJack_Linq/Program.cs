using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackJack_Linq
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
        //マーク
        enum Suit { diamond, club, heart, spade }
        //カード
        class Card
        {
            //1~13の数字 読み取り専用
            public int No { get; }

            //マーク　読み取り専用
            public Suit Suit { get; }

            //絵柄　読み取り専用
            //三項演算子
            public string Rank =>
                No == 1 ? "A" :
                No == 11 ? "J" :
                No == 12 ? "Q" :
                No == 13 ? "K" :
                No.ToString();

            //表か裏か　
            public bool FaceUp { get; set; }

            public Card(Suit suit, int no)
            {
                if (no < 1 || 13 < no) throw new ArgumentOutOfRangeException(nameof(no));
                this.No = no;
                this.Suit = suit;
            }

            //カード表示用
            public override string ToString()
            {
                string suit;
                string rank;
                if (FaceUp)
                {
                    suit = Suit.ToString();
                    rank = Rank.ToString();
                }
                else
                {
                    suit = "???????";
                    rank = "??";
                }

                return $"[{suit,7}|{rank,2}]";
            }
        }

        //デッキ
        class Deck
        {
            //内部的にカードをスタックして保持する
            private Stack<Card> Cards { get; }

            public Deck()
            {
                var newCards = CreateCards();
                var shuffled = Shuffle(newCards);
                //https://qiita.com/Marimoiro/items/1509f7c1d4823a52b2f5
                //指定したコレクションからコピーした要素を格納し、コピーされる要素の数を格納できるだけの容量を備えた、Stack<T> クラスの新しいインスタンスを初期化します。
                //https://docs.microsoft.com/ja-jp/dotnet/api/system.collections.generic.stack-1.-ctor?view=netframework-4.8
                Cards = new Stack<Card>(shuffled);
            }
            //デッキの先頭からカードを一枚取り出す
            public Card Pop()
            {
                return Cards.Pop();
            }
            //public Card Pop() => return Cards.Pop();

            //新規に５２枚のカードを用意する
            //コレクションに対する単純な反復処理をサポートする反復子を公開する
            //https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q1191385706
            private IEnumerable<Card> CreateCards()
            {
                var suits = GetSuitValues();
                var numbers = Enumerable.Range(1, 13);
                //平坦化して取得
                var cards = suits.SelectMany(
                    suit => numbers.Select(no => new Card(suit, no)));
                return cards;
            }

            //トランプのマークをすべて取得する
            //列挙型のSuitをIEnumerable<Suit>型に
            private IEnumerable<Suit> GetSuitValues() =>
                Enum.GetValues(typeof(Suit)).Cast<Suit>();

            // カードをシャッフルする
            private IEnumerable<Card> Shuffle(IEnumerable<Card> cards)
            {
                var random = new Random();
                //乱数によってソート
                var shuffled = cards.OrderBy(_ => random.Next());
                return shuffled;
            }
        }
    }
}
