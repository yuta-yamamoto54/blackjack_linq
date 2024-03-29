﻿//参考引用：https://qiita.com/Nossa/items/b9298393439003997ed1
//https://qiita.com/hirossyi73/items/cf8648c31898216312e5
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;


namespace BlackJack_Linq
{
    //エントリーポイント
    class Program
    {
        static void Main(string[] args)
        {
            var deck = new Deck();
            var player = new Player(new Hand(), "Player");
            var dealer = new Player(new Hand(), "Dealer");
            var game = new Game(player, dealer, deck);
            game.Run();
        }
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

    //手札
    class Hand
    {
        //内部的にカードのリストとして保持する
        private IList<Card> Cards { get; }
        //インターフェースIList<T>のインスタンスは作成できない　https://www.atmarkit.co.jp/fdotnet/csharp_abc/csharp_abc_004/csharp_abc03.html
        //https://www.slideshare.net/ryotamurohoshi/clistlttilist
        public Hand() => Cards = new List<Card>();

        //カードを一枚加える
        public void Add(Card card) => Cards.Add(card);

        //点数を決める
        public int ComputeScore()
        {
            var sum = Cards.Sum(card => card.No > 10 ? 10 : card.No);
            if (ContainsAce && sum <= 11)
            {
                sum += 10;
            }
            return sum;
        }

        //エースが含まれているか？
        private bool ContainsAce =>
            Cards.Any(card => card.No == 1);

        //手札の内容表示用
        //join の説明　https://qiita.com/pierusan2010/items/f5f1487c03561cf4ec9f
        //select の説明　https://www.sejuku.net/blog/47172
        public override string ToString() =>
            string.Join(' ', Cards.Select(card => card.ToString()));

        public void FaceUpAll()
        {
            foreach (var card in Cards)
            {
                card.FaceUp = true;
            }
        }
    }

    //プレイヤークラス
    class Player
    {
        private Hand Hand { get; }
        
        //名前
        public string Name { get; }
        //得点
        public int Score => Hand.ComputeScore();

        //バーストしているか
        public bool IsBust => Score > 21;

        public Player(Hand hand, string name)
        {
            Hand = hand;
            Name = name;
        }
        //カードを一枚引く
        public void Take(Card card, bool faceUp = true)
        { 
            card.FaceUp = faceUp;
            ShowTookCard(card);
            Hand.Add(card);
        }

        //カードの表示
        private void ShowTookCard(Card card) =>
            WriteLine($"[{Name}] => {card}");

        //手札を表示する
        public void ShowHand()
        {
            Hand.FaceUpAll();
            WriteLine($"[{Name}] => Hand: {Hand}");
            WriteLine($"[{Name}] => Score: {Score}");
        }
    }

    //ゲームクラス ゲームがデッキとプレイヤーとディーラーを持つ
    class Game
    {
        Player Player { get; }
        Player Dealer { get; }
        private Deck Deck { get; }

        public Game(Player player, Player dealer,Deck deck)
        {
            Player = player;
            Dealer = dealer;
            Deck = deck;
        }

        public void Run()
        {
            WriteLine("<< Welcome to Blackjack!! >>\n");

            //プレイヤー最初のドロー
            Player.Take(Deck.Pop());
            Player.Take(Deck.Pop());
            Player.ShowHand();
            WriteLine();

            //ディーラーの最初のドロー
            Dealer.Take(Deck.Pop());
            Dealer.Take(Deck.Pop(),faceUp: false);
            WriteLine();

            //プレイヤーのターン
            WriteLine($"<< {Player.Name} turn! >>");
            while (ConfirmHitOrStand("Hit or Stand?", 'h', 's'))
            {
                Player.Take(Deck.Pop());
                Player.ShowHand();
                if (Player.IsBust)
                {
                    WriteLine($"{Player.Name} have over 21, {Player.Name} bust!");
                    WriteLine();
                    Lost();
                }
            }
            WriteLine();

            //ディーラーのターン
            WriteLine($"<< {Dealer.Name} turn! >>");
            while (Dealer.Score < 17)
            {
                Dealer.Take(Deck.Pop());
            }
            Dealer.ShowHand();
            if (Dealer.IsBust)
            {
                WriteLine($"{Dealer.Name} have over 21, {Dealer.Name} bust!");
                WriteLine();
                Won();
            }
            WriteLine();

            //判定
            WriteLine("<< Result >>");
            Player.ShowHand();
            Dealer.ShowHand();
            if (Player.Score > Dealer.Score)
            {
                Won();
            }
            else if (Dealer.Score > Player.Score)
            {
                Lost();
            }
            else
            {
                Drawn();
            }

            //ヒット・スタンド確認
            bool ConfirmHitOrStand(string message, char hit, char stand)
            {
                while (true)
                {
                    Write($"{message} [{hit}/{stand}]");
                    //keycharでUNICODE文字を出す
                    var key = ReadKey().KeyChar;
                    WriteLine();
                    if (key == hit)
                    {
                        return true;
                    }
                    if (key == stand)
                    {
                        return false;
                    }
                    WriteLine($"Invalid key. Please input {hit} or {stand}.");
                }
            }

            //勝ち
            void Won()
            {
                WriteLine($"{Player.Name} won. Congrats!");
                End();
            }

            //負け
            void Lost()
            {
                WriteLine($"{Player.Name} lost.");
                End();
            }

            //引き分け
            void Drawn()
            {
                WriteLine("This game was drawn...");
                End();
            }

            //終了
            void End()
            {
                WriteLine("To close, press any key.");
                //trueなら表示しない
                ReadKey(intercept: true);
                Environment.Exit(0);
            }

        }
    }
}
