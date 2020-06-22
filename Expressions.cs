using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Expressions {
	/// <summary>文字列で表記された数式と論理式を評価する機能を提供します。</summary>
	[System.Runtime.CompilerServices.CompilerGenerated]
  internal class NamespaceDoc { }

  /// <summary>
  /// 正規表現で使用するパターンを提供します。
  /// </summary>
  public static class Patterns {
    //------------------------------
    /// <summary>
    /// 日時を記述します。
    /// </summary>
    public const string Stamp = @"^\d{4}[/-]\d{1,2}[/-]\d{1,2}[\sT]?\d{1,2}[:-]\d{1,2}[:-]\d{1,2}$";

    //------------------------------
    /// <summary>
    /// 浮動小数点を記述します。
    /// </summary>
    public const string Float = @"^[+\-]?\d*\.?\d+([eE][-+]?\d+)?$";

    //------------------------------
    /// <summary>
    /// 整数を記述します。
    /// </summary>
    public const string Integer = @"^[+\-]?\d*$";

    //------------------------------
    /// <summary>
    /// 真偽を記述します。
    /// </summary>
    public const string Boolean = @"(?<bool>^([Tt][Rr][Uu][Ee]|[Ff][Aa][Ll][Ss][Ee])$)";

    //------------------------------
    /// <summary>
    /// 数値を記述します。
    /// </summary>
    public const string Numeric = @"(?<numeric>(^[+\-]{1})?\d+(\.?\d+)?([eE][+-]?\d+)?)";

    //------------------------------
    /// <summary>
    /// 符号無しの数値を記述します。
    /// </summary>
    public const string NumericWithoutSign = @"(?<numeric>\d+(\.?\d+)?([eE][+-]?\d+)?)";

    //------------------------------
    /// <summary>
    /// 比較子を記述します。
    /// </summary>
    public const string Comparator = @"(?<comparator>(=|==|>|>=|<|<=|!=){1})";

    //------------------------------
    /// <summary>
    /// 比較式を記述します。
    /// </summary>
    public const string Logic = @"(?<logic>(?<left>[\w\.:]*)?" + Comparator + @"(?<right>[\w+]+))";

    //------------------------------
    /// <summary>
    /// 四則演算の演算子を記述します。
    /// </summary>
    public const string OperatorArithmetic = @"(?<operator>[\+\-\*\/]{1})";

    //------------------------------
    /// <summary>
    /// 論理演算の演算子を記述します。
    /// </summary>
    public const string OperatorLogic = @"(?<operator>[&\^\|]{1})";

    //------------------------------
    /// <summary>
    /// 関数を記述する正規表現。
    /// </summary>
    public const string Function = @"^(?<function>[+\-]{1}[\w]+)";

  }//end of static class Patterns

  //====================================================================================
  /// <summary>
  /// 比較演算子タイプを定義します。
  /// </summary>
  public enum ComparatorType {
    /// <summary>
    /// 無効な比較演算子
    /// </summary>
    Null,
    /// <summary>
    /// 等しい「=」または「==」
    /// </summary>
    Equal,
    /// <summary>
    /// 異なっている「!=」
    /// </summary>
    NotEqual,
    /// <summary>
    /// 大きい
    /// </summary>
    Larger,
    /// <summary>
    /// 等しいか大きい
    /// </summary>
    LargerThan,
    /// <summary>
    /// 小さい
    /// </summary>
    Lower,
    /// <summary>
    /// 等しいか小さい
    /// </summary>
    LowerThan
  }//end of enum ComparatorType

  //====================================================================================
  /// <summary>
  /// 式を構成する要素の種類を定義します。
  /// </summary>
  public enum TokenType {
    /// <summary>
    /// 論理
    /// </summary>
    Logic,
    /// <summary>
    /// 数値
    /// </summary>
    Numeric,
    /// <summary>
    /// 変数
    /// </summary>
    Variable,
    /// <summary>
    /// 演算子
    /// </summary>
    Operator,
    /// <summary>
    /// 関数
    /// </summary>
    Function
  }//end o fo enum TokenType

  //====================================================================================
  /// <summary>
  /// 式を構成する要素を格納します。
  /// </summary>
  public class Token {
    #region フィールド

    //------------------------------
    /// <summary>
    /// トークンタイプを取得または設定します。
    /// </summary>
    public TokenType Type { get; set; }

    //------------------------------
    /// <summary>
    /// トークンの値を取得または設定します。
    /// </summary>
    public dynamic Value { get; set; }

    //------------------------------
    /// <summary>
    /// トークンのキーを取得または設定します。
    /// </summary>
    public string Key { get; set; }

    //------------------------------
    /// <summary>
    /// トークンが処理するフィールド数を取得または設定します。
    /// </summary>
    public int NumberOfParameter { get; set; }

    #endregion

    #region メソッド

    //------------------------------
    /// <summary>
    /// Tokenを初期化します。
    /// </summary>
    /// <param name="Type">トークンタイプを指定します。</param>
    /// <param name="Value">トークンの値を指定します。</param>
    /// <param name="Key">トークンのキーを指定します。</param>
    /// <param name="NumberOfParameter">トークンが処理するフィールド数を指定します。</param>
    public Token(TokenType Type, dynamic Value, string Key = "", int NumberOfParameter = 0) {
      this.Type = Type;
      this.Value = Value;
      this.Key = Key != "" ? Key : $"{Value}";
      this.NumberOfParameter = NumberOfParameter;
    }

    //------------------------------
    /// <summary>
    /// トークンの内容を文字列で返します。
    /// </summary>
    /// <returns>トークンを示す文字列を返します。</returns>
    public override string ToString() {
      return $@"{Type}, {Key}, {NumberOfParameter}";
    }

    #endregion
  }//end of class Token

  //====================================================================================
  /// <summary>
  /// 文字列で与えられた数式を評価します。後置記法への変換は<a href="http://7ujm.net/etc/calcstart.html">このページ</a>を参照して構築しました。
  /// </summary>
  /// <example>
  /// MathExpressionの使用例を示します。
  /// <code>
  /// void TestMathExpression() {
  ///   string Conversion = "-1 + sin(4+(x+pi()))*3.2E+3 - 6*(exp(-3.2*power((x-3)/e(),2))+2.4)";//数式を指定します。空白は除去されます。
  ///   RadonLab.Formula.MathExpression Formula = new RadonLab.Formula.MathExpression(Conversion);//MathExpressionを初期化します。
  ///   
  ///   if(Formula != null) {
  ///     Console.WriteLine($@"{Formula.Expression}");//入力された数式を出力します。
  ///     Console.WriteLine($@"{Formula.Tokenized}");//後置記法で数式を出力します。-1,4,x,pi(),+,+,sin,3.2e+3,*,+,6,-3.2,x,3,-,e(),/,2,power,*,exp,2.4,+,*,-
  ///     Console.WriteLine($@"{Formula.Aggregated}");//中置記法に再構成した数式を出力します。((-1+(sin((4+(x+pi())))*3.2e+3))-(6*(exp((-3.2*power(((x-3)/e()),2)))+2.4)))
  ///   
  ///     double Result = Formula.Evaluate(3.5);//変数xに3.5を代入して式の値を計算します。結果は-3022.3842468459384になります。
  ///   }
  /// }
  /// </code>
  /// </example>
  /// <example>
  /// MathExpressionに複数変数を指定する使用例を示します。MathExpressionで使用できる関数は<see cref="System.Math"/>も参照して下さい。
  /// <code>
  /// void TestMathExpression() {
  ///   string Conversion = "1/(sqrt(2*pi())*2)*exp(-power((x-1)/2,2)/2)";//これはガウス関数です。
  ///   RadonLab.Formula.MathExpression Formula = new RadonLab.Formula.MathExpression(Conversion);//後置記法では、[0],[1],[2],*,sqrt,[3],*,/,[4],x,[5],-,[6],/,[7],power,*,[8],/,exp,* と表現されます。
  ///   if(Formula != null)
  ///     double Result = Formula.Evaluate(0.1);//結果は0.180263481230824になります。
  ///     
  ///   Conversion = "gauss(x;a,b,c)";//ガウス関数はxの他に、高さ:a、中心値:b、半値半幅:cを指定します。
  ///   Formula = new RadonLab.Formula.MathExpression(Conversion);//後置記法では、x,a,b,c,gauss と表現されます。
  ///   if(Formula != null)
  ///     double Result = Formula.Evaluate(0.1, new(string, double)[] { ("a", 1), ("b", 1), ("c", 2) });//結果は0.180263481230824になります。
  /// }
  /// </code>
  /// </example>
  /// <remarks>
  /// MathExpressionで使用できる関数は以下の通りです。<see cref="System.Math"/>も参照して下さい。
  /// 
  /// <table>
  ///   <th>関数名</th><th>引数</th><th>説明</th>
  ///   <tr><td colspan="3"><center><b>定数関数</b></center></td></tr>
  ///   <tr><td>pi()</td><td></td><td>πを返します。</td></tr>
  ///   <tr><td>e()</td><td></td><td>自然対数の底eを返します。exp(1)と同等です。</td></tr>
  ///   <tr><td colspan="3"><b>1変数関数</b></td></tr>
  ///   <tr><td>exp(x)</td><td>x</td><td>底をeとするxの累乗を返します。</td></tr>
  ///   <tr><td>ln(x)</td><td>x</td><td>底をeとするxの対数を返します。</td></tr>
  ///   <tr><td>log(x)</td><td>x</td><td>底を10とするxの対数を返します。</td></tr>
  ///   <tr><td>sqrt(x)</td><td>x</td><td>平方根を返します。</td></tr>
  ///   <tr><td>abst(x)</td><td>x</td><td>絶対値を返します。</td></tr>
  ///   <tr><td>sin(x)</td><td>x</td><td>sinを返します。xはラジアンで与えます。</td></tr>
  ///   <tr><td>cos(x)</td><td>x</td><td>cosを返します。xはラジアンで与えます。</td></tr>
  ///   <tr><td>tan(x)</td><td>x</td><td>tanを返します。xはラジアンで与えます。</td></tr>
  ///   <tr><td>asin(x)</td><td>x</td><td>asinを返します。xは求める角度のsin値で与えます。</td></tr>
  ///   <tr><td>acos(x)</td><td>x</td><td>acosを返します。xは求める角度のcos値で与えます。</td></tr>
  ///   <tr><td>atan(x)</td><td>x</td><td>atanを返します。xは求める角度のtan値で与えます。</td></tr>
  ///   <tr><td>sinh(x)</td><td>x</td><td>sinhを返します。(exp(x)-exp(-x))/2と同等です。</td></tr>
  ///   <tr><td>cosh(x)</td><td>x</td><td>coshを返します。(exp(x)+exp(-x))/2と同等です。</td></tr>
  ///   <tr><td>tanh(x)</td><td>x</td><td>tanhを返します。sinh(x)/cosh(x)と同等です。</td></tr>
  ///   <tr><td>truncate(x)</td><td>x</td><td>小数部を切り捨てたxの整数部を返します。</td></tr>
  ///   <tr><td>sign(x)</td><td>x</td><td>xの符号を返します。正なら1、負なら-1です。</td></tr>
  ///   <tr><td>floor(x)</td><td>x</td><td>xを越えない最大の整数値を返します。</td></tr>
  ///   <tr><td>ceiling(x)</td><td>x</td><td>x以上の数のうち、最小の整数値を返します。</td></tr>
  ///   <tr><td>round(x)</td><td>x</td><td>xの最も近い整数値に丸められ、中間値は最も近い偶数値に丸められます。</td></tr>
  ///   <tr><td>sinc(x)</td><td>x</td><td>sincを返します。x=0の値は1です。sinc関数は幅2πで単位高さ1の、0を中心とした周波数成分を持つ矩形パルスの逆フーリエ変換です。</td></tr>
  ///   <tr><td colspan="3"><b>2変数関数</b></td></tr>
  ///   <tr><td width="150">power(x;a), pow(x;a)</td><td>x,a</td><td>xのa乗(x^a)を返します。</td></tr>
  ///   <tr><td>mod(x;a)</td><td>x,a</td><td>xをaで割ったときの余りを返します。</td></tr>
  ///   <tr><td colspan="3"><b>3変数関数</b></td></tr>
  ///   <tr><td>decay(x;a,b)</td><td>x,a,b</td><td>指数減衰関数(x=0の値a,減衰率b)の、xでの値を返します。a*exp(-x/b)と同等です。</td></tr>
  ///   <tr><td colspan="3"><b>4変数関数</b></td></tr>
  ///   <tr><td>stretched(x;a,b,c)</td><td>x,a,b,c</td><td><a href="https://en.wikipedia.org/wiki/Stretched_exponential_function">伸張指数減衰関数</a>(x=0の値a,減衰率b,xの指数c)の、xでの値を返します。a*exp(-power(x,c)/b)と同等です。</td></tr>
  ///   <tr><td>gauss(x;a,b,c)</td><td>x,a,b,c</td><td><a href="https://en.wikipedia.org/wiki/Gaussian_function">ガウス関数</a>(高さa,中心値b,半値半幅c)の、xでの値を返します。a/(sqrt(2*pi()*c)*exp(-power((x-b)/c,2)/2)と同等です。</td></tr>
  ///   <tr><td>lorentz(x;a,b,c)</td><td>x,a,b,c</td><td><a href="https://en.wikipedia.org/wiki/Cauchy_distribution">ローレンツ関数</a>(高さa,中心値b,半値半幅c)の、xでの値を返します。a/pi()*c/(power(x-b,2)+power(c,2))と同等です。</td></tr>
  ///   <tr><td colspan="3"><b>5変数関数</b></td></tr>
  ///   <tr><td>voigt(x;a,b,c,d)</td><td>x,a,b,c,d</td><td><a href="https://en.wikipedia.org/wiki/Voigt_profile">疑似フォークト関数</a>(高さa,中心値b,半値半幅c,ガウス関数の比率d)、xでの値を返します。d*gauss(x;a,b,c)+(1-d)*lorentz(x;a,b,c)と同等です。</td></tr>
  /// </table>
  /// </remarks>
  public class MathExpression {
    #region フィールド

    //------------------------------
    /// <summary>
    /// 入力された式を取得します。
    /// </summary>
    public string Expression { get; private set; }

    //------------------------------
    /// <summary>
    /// 詳細モードを取得または設定します。
    /// </summary>
    public bool IsVerbose { get; set; }

    //------------------------------
    /// <summary>
    /// 式の構成要素リストを取得します。
    /// </summary>
    public List<Token> Tokens { get; private set; }

    #endregion

    #region メソッド

    //------------------------------
    /// <summary>
    /// MathExpressionを初期化します。
    /// </summary>
    /// <param name="Expression">数式を指定します。変数は1文字のアルファベットで記述します。</param>
    /// <param name="IsVerbose">詳細モードを指定します。</param>
    public MathExpression(string Expression, bool IsVerbose = false) {
      this.IsVerbose = IsVerbose;
      this.Expression = Regex.Replace(Expression, @"\s", "").ToLower();//空白を削除
      List<dynamic> Constants = new List<dynamic>();//定数リストを初期化
      Tokens = Activate(Tokenize(Evacuate(this.Expression, ref Constants)), Constants);//式をトークン化
    }

    //------------------------------
    /// <summary>
    /// 式中の数値をタグ[\d]で退避します。
    /// </summary>
    /// <param name="Expression">数式を指定します。</param>
    /// <param name="Constants">定数要素リストを指定します。</param>
    /// <param name="Depth">詳細モードの時に使用するインデントの深さを指定します。</param>
    /// <returns>定数を置換した後の数式を文字列で返します。</returns>
    string Evacuate(string Expression, ref List<dynamic> Constants, int Depth = 0) {
      const string Physical = @"(?<physics>([pP][iI]\(\)|[eE]\(\)))";//物理定数
      const string Brackets = @"(?<leftbracket>\({1})";//括弧
      const string Delimiter = @"(?<delimiter>[,;:]{1})";//デリミタ

      string Indents = new string('\t', Depth);//詳細モード用インデント

      List<dynamic> LocalConstants = new List<dynamic>();
      string Evacuated = "";//定数置換された式
      string Buffer = Expression;
      int LoopCount = 0;

      int BracketPosition, BracketCount;//括弧を検索する変数
      string EnclosedArgument;//括弧に囲まれた式を格納するバッファ

      if(IsVerbose)
        Console.WriteLine($@"{DateTime.Now} | {Indents}数式 {Expression} の定数をタグ[\d]で置換します。 @MathExpression");

      while(0 < Expression.Length) {
        string Numeric = LoopCount == 0 ? Patterns.Numeric : Patterns.NumericWithoutSign;//Constantsが空の時は先頭に-がある可能性がある
        string Function = LoopCount == 0 ? "|" + @"^(?<function>[+\-]{1}[\w]+)" : "";
        var Matched = Regex.Match(Expression, $@"({Numeric}|{Physical}|{Brackets}|{Delimiter}{Function})", RegexOptions.IgnoreCase);
        if(Matched.Success) {
          var Captures = Matched.Groups.Cast<Group>().Where(m => m.Success == true & !int.TryParse(m.Name, out int r));//マッチコレクションの要素のなかで、Successがtureで名前が数値でないもの、を取り出す。
          if(Captures.Count() == 1) {//念のため、キャプチャ数が1であることを確かめる
            var Captured = Captures.First();//先頭がキャプチャ情報になっている
            if(IsVerbose)
              Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}""{Captured.Value}""が捕捉されました。");
            switch(Captured.Name) {//キャプチャ名によって処理を分岐する
              case "numeric":
                Evacuated += $@"{Expression.Substring(0, Captured.Index)}[{Constants.Count}]";
                Expression = Expression.Substring(Captured.Index + Captured.Value.Length);//定数を元の式から削除
                Constants.Add(Captured.Value);
                LocalConstants.Add(Captured.Value);
                break;
              case "physics":
                Evacuated += $@"{Expression.Substring(0, Captured.Index)}[{Constants.Count}]";
                Expression = Expression.Substring(Captured.Index + Captured.Value.Length);//定数を元の式から削除
                Constants.Add(Captured.Value);
                LocalConstants.Add(Captured.Value);
                break;
              case "function"://Expressionの先頭に「-」がついた関数が見つかったときだけここへ来る。上のFunctionが有効になるのは初回のループだけ
                Evacuated += $@"[{Constants.Count}]";
                Expression = $@"*{Expression.Substring(1)}";//先頭の「-」を「*」に替える
                Constants.Add("-1");
                LocalConstants.Add("-1");
                break;
              case "delimiter":
                LoopCount = -1;
                Evacuated += $@"{Expression.Substring(0, Captured.Index)}{Captured.Value}";
                Expression = Expression.Substring(Captured.Index + Captured.Value.Length);//.ValueBracketPosition + 1);//対応括弧までの範囲を元の式から削る
                break;
              case "leftbracket":
                BracketPosition = Captured.Index;//括弧位置
                BracketCount = 0;//括弧数
                while(true) {
                  if(Expression[BracketPosition] == '(')//左括弧なら
                    BracketCount++;//括弧数を増やす
                  if(Expression[BracketPosition] == ')')//右括弧なら
                    BracketCount--;//括弧数を減らす
                  if(BracketCount == 0)//括弧数がゼロになったら
                    break;//対応した括弧の位置に到達
                  BracketPosition++;//位置をずらす
                }
                if(IsVerbose)
                  Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}対応する"")""が捕捉されました。");
                EnclosedArgument = Expression.Substring(Captured.Index + 1, BracketPosition - Captured.Index - 1);//括弧の中の式を取り出して
                if(IsVerbose)
                  Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}{EnclosedArgument} を処理します...");
                Evacuated += $@"{Expression.Substring(0, Captured.Index)}({Evacuate(EnclosedArgument, ref Constants, Depth + 1)})";
                Expression = Expression.Substring(BracketPosition + 1);//対応括弧までの範囲を元の式から削る
                break;
            }
          }
        } else
          break;
        LoopCount++;
      }

      if(IsVerbose) {
        Console.WriteLine($@"{DateTime.Now} | {Indents}処理された式は {Evacuated + Expression} です。");
        if(Depth == 0)
          Console.WriteLine($@"{DateTime.Now} | {Indents}抽出された定数は {string.Join(",", Constants.Select((c, i) => $@"[{i}]={c}"))} です。");
      }
      return Evacuated + Expression;
    }

    //------------------------------
    /// <summary>
    /// 式中の数値がタグ[\d]で置換された数式を構成要素に分解します。
    /// </summary>
    /// <param name="Expression">定数がタグ化された数式を指定します。</param>
    /// <param name="Depth">詳細モードの時に使用するインデントの深さを指定します。</param>
    /// <returns>構成要素リストを返します。</returns>
    List<Token> Tokenize(string Expression, int Depth = 0) {
      const string Constant = @"(?<constant>\[\d+\])";//置換された定数
      const string Operator = @"(?<operator>[\+\-\*\/]{1})";//四則演算の演算子
      const string Variable = @"\b(?<variable>[a-zA-Z]{1})\b";//一文字のアルファベットは変数として扱う
      const string Function = @"\b(?<function>[+\-]?[\w]{2,})";//括弧が後ろに続く二文字以上の文字で構成された関数。Mathに定義されていないものでも関数名として取り出す
      const string Brackets = @"(?<leftbracket>\({1})";//括弧
      const string Delimiter = @"(?<delimiter>[,;:]{1})";//デリミタ

      string Indents = new string('\t', Depth);//詳細モード用インデント

      List<Token> Tokenized = new List<Token>();//後置されたトークンリスト

      Regex Parser = new Regex($@"({Constant}|{Operator}|{Variable}|{Function}|{Brackets}|{Delimiter})", RegexOptions.IgnoreCase);//正規表現パーサ
      List<string> Stack = new List<string>();//後置処理用のスタック
      int BracketPosition, BracketCount;//括弧を検索する変数
      string EnclosedArgument;//括弧に囲まれた式を格納するバッファ

      if(IsVerbose)
        Console.WriteLine($@"{DateTime.Now} | {Indents}数式 {Expression} を後置処理します。");
      while(0 < Expression.Length) {//式がすべて処理されるまで続ける
        var Matched = Parser.Match(Expression);//文字列の先頭から要素を検索し
        if(Matched.Success) {//要素が見つかれば
          var Captures = Matched.Groups.Cast<Group>().Where(m => m.Success == true & !int.TryParse(m.Name, out int r));//マッチコレクションの要素のなかで、Successがtureで名前が数値でないもの、を取り出す。
          if(Captures.Count() == 1) {//念のため、キャプチャ数が1であることを確かめる
            var Captured = Captures.First();//先頭がキャプチャ情報になっている
            if(IsVerbose)
              Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}""{Captured.Value}""が捕捉されました。 @MathExpression");
            switch(Captured.Name) {//キャプチャ名によって処理を分岐する
                                   //-----------------------------------------------
              case "constant"://数値なら
                Tokenized.Add(new Token(TokenType.Numeric, Captured.Value));//トークンリストに出力する
                Expression = Expression.Substring(Matched.Value.Length);//キャプチャ定数を元の式から削除
                break;
              //-----------------------------------------------
              case "operator"://演算子で
                if(Stack.Count == 0)//スタックが空なら積む
                  Stack.Add(Captured.Value);
                else {//スタックが空でないなら
                  while(Stack.Count > 0 && Priority(Captured.Value) <= Priority(Stack.First())) { //キャプチャ演算子の優先度がスタック先頭と同じか低い間は
                    Tokenized.Add(new Token(TokenType.Operator, Stack.First(), NumberOfParameter: 2));//スタックから取りだしてトークンリストに追加する
                    Stack.RemoveAt(0);//スタックの先頭を削除する
                  }
                  Stack.Insert(0, Captured.Value);//キャプチャ演算子をスタックに積む
                }
                Expression = Expression.Substring(Matched.Value.Length);//キャプチャ演算子を元の式から削除
                break;
              //-----------------------------------------------
              case "variable":
                Tokenized.Add(new Token(TokenType.Variable, Captured.Value));//トークンリストに追加する
                Expression = Expression.Substring(Matched.Value.Length);//変数を元の式から削除
                break;
              //-----------------------------------------------
              case "function"://関数なら
                if(IsVerbose)
                  Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}""(""が捕捉されました。 @MathExpression");
                Expression = Expression.Substring(Captured.Value.Length);//関数名を元の式から削除
                BracketPosition = 0;//確固位置
                BracketCount = 0;//括弧数
                while(true) {
                  if(Expression[BracketPosition] == '(')//左括弧なら
                    BracketCount++;//括弧数を増やす
                  if(Expression[BracketPosition] == ')')//右括弧なら
                    BracketCount--;//括弧数を減らす
                  if(BracketCount == 0)//括弧数がゼロになったら
                    break;//対応した括弧の位置に到達
                  BracketPosition++;//位置をずらす
                }
                if(IsVerbose)
                  Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}対応する"")""が捕捉されました。 @MathExpression");
                EnclosedArgument = Expression.Substring(1, BracketPosition - 1);//括弧の中の式を取り出して
                Expression = Expression.Substring(BracketPosition + 1);//対応括弧までの範囲を元の式から削除
                foreach(var Item in Tokenize(EnclosedArgument, Depth + 1))//括弧内の式をトークン化して
                  Tokenized.Add(Item);//トークン列に追加する
                switch(Captured.Value.ToLower()) {
                  case "foigt"://疑似フォークト関数(x;a,b,c,d)　a:高さ,b:平均値,c:半値半幅,d:ガウス関数の混合率(0-1)　a*(d/(sqrt(2*pi())*c)*exp(-power((x-b)/c,2)/2) + (1-d)*1/pi()*c/(power(x-b,2)+power(c,2)))
                    Tokenized.Add(new Token(TokenType.Function, Captured.Value, NumberOfParameter: 5));//4変数演算子としてスタックに積む
                    break;
                  case "gauss"://ガウス関数(x;a,b,c)　a:高さ,b:平均値,c:半値半幅　a/(sqrt(2*pi())*c)*exp(-power((x-b)/c,2)/2)
                  case "lorentz"://ローレンツ関数(x;a,b,c)　a:高さ,b:平均値,c:半値半幅　a/pi()*c/(power(x-b,2)+power(c,2))
                  case "stretched"://拡張指数関数(x;a,b,c)　a:初期値,b:減衰係数,c:伸張指数(0-1)　a*exp(-power(x,c)/b)
                    Tokenized.Add(new Token(TokenType.Function, Captured.Value, NumberOfParameter: 4));//4変数演算子としてスタックに積む
                    break;
                  case "decay"://指数減衰関数(x;a,b)　a:初期値,b:減衰係数　a*exp(-x/b)
                    Tokenized.Add(new Token(TokenType.Function, Captured.Value, NumberOfParameter: 3));//3変数演算子としてスタックに積む
                    break;
                  case "power"://べき関数(x;a)　a:べき数　power(x;a)
                    Tokenized.Add(new Token(TokenType.Function, Captured.Value, NumberOfParameter: 2));//2変数演算子としてスタックに積む
                    break;
                  default:
                    Tokenized.Add(new Token(TokenType.Function, Captured.Value, NumberOfParameter: 1));//1変数演算子としてスタックに積む
                    break;
                }
                break;
              //-----------------------------------------------
              case "leftbracket"://関数に属さない独立した開く括弧なら
                BracketPosition = 0;//確固位置
                BracketCount = 0;//括弧数
                while(true) {
                  if(Expression[BracketPosition] == '(')//左括弧なら
                    BracketCount++;//括弧数を増やす
                  if(Expression[BracketPosition] == ')')//右括弧なら
                    BracketCount--;//括弧数を減らす
                  if(BracketCount == 0)//括弧数がゼロになったら
                    break;//対応した括弧の位置に到達
                  BracketPosition++;//位置をずらす
                }
                if(IsVerbose)
                  Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}対応する"")""が捕捉されました。 @MathExpression");
                EnclosedArgument = Expression.Substring(1, BracketPosition - 1);//括弧の中の式を取り出して
                Expression = Expression.Substring(BracketPosition + 1);//対応括弧までの範囲を元の式から削る
                foreach(var Item in Tokenize(EnclosedArgument, Depth + 1))//括弧内の式をトークン化して
                  Tokenized.Add(Item);//トークン列に加える
                break;
              case "delimiter"://2変数以上の関数内のデリミタ(,;)なら
                while(Stack.Count > 0 && Priority(Captured.Value) <= Priority(Stack.First())) { //キャプチャ演算子の優先度がスタック先頭と同じか低い間は
                  Tokenized.Add(new Token(TokenType.Operator, Stack.First(), NumberOfParameter: 2));//スタックから取りだしてトークンリストに追加する
                  Stack.RemoveAt(0);//スタックの先頭を削除する
                }
                Expression = Expression.Substring(Matched.Value.Length);//デリミタを元の式から削除する
                break;
            }
          }
        }
      }

      //式がすべて処理されたら
      foreach(var Item in Stack)//スタックに残っている演算子を
        Tokenized.Add(new Token(TokenType.Operator, Item, NumberOfParameter: 2));//トークンリストに追加する
      if(IsVerbose)
        Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}後置処理された結果は {string.Join(",", Tokenized.Select(v => v.Value))} です。 @MathExpression");

      return Tokenized;
    }

    //------------------------------
    /// <summary>
    /// タグ[\d]で退避した数値を構成要素リストに戻します
    /// </summary>
    /// <param name="Tokenized">構成要素リストを指定します。</param>
    /// <param name="Constants">定数リストを指定します。</param>
    /// <returns>構成要素リストを返します。</returns>
    List<Token> Activate(List<Token> Tokenized, List<dynamic> Constants) {
      for(int Index = 0; Index < Tokenized.Count; Index++) {
        if(Tokenized[Index].Type == TokenType.Numeric) {
          int Jndex = int.Parse(Regex.Replace(Tokenized[Index].Value, @"[\[\]]", ""));
          Tokenized[Index].Key = Constants[Jndex];
          switch(Constants[Jndex].ToLower()) {
            case "pi()":
              Tokenized[Index].Value = Math.PI;
              break;
            case "e()":
              Tokenized[Index].Value = Math.E;
              break;
            default:
              Tokenized[Index].Value = double.Parse(Constants[Jndex]);
              break;
          }
          if(IsVerbose)
            Console.WriteLine($@"{DateTime.Now} | ""{Tokenized[Index].Key}""={Tokenized[Index].Value}. @MathExpression");
        }
      }
      return Tokenized;
    }

    //------------------------------
    /// <summary>
    /// 演算子の優先度を返します。
    /// </summary>
    /// <param name="Operator">演算子を指定します。</param>
    /// <returns>優先度を返します。</returns>
    int Priority(string Operator) {
      if(Operator == "+" | Operator == "-")//和と差は0
        return 0;
      else if(Operator == "*" | Operator == "/")//積と商は2
        return 2;
      else//そのほかの関数は1
        return 1;
    }

    //------------------------------
    /// <summary>
    /// 数式を計算します。
    /// </summary>
    /// <param name="x">数式の変数xに代入する値を指定します。</param>
    /// <param name="Auxiliary">数式の補助変数(a～w,y,z)に代入する値の配列を、(名前,値)の配列で指定します。</param>
    /// <returns>計算結果を返します。</returns>
    public double Evaluate(double x, (string Name, double Value)[] Auxiliary = null) {
      if(Tokens.Count > 0) {
        double a, b, c, d;
        List<Token> Evaluator = new List<Token>(Tokens);
        Token Processed = null;

        //xは必ずあるのでxの値をセット
        foreach(var Index in Evaluator.Select((m, i) => new { index = i, value = m }).Where(v => v.value.Type == TokenType.Variable & v.value.Key.ToLower() == "x").Select(v => v.index))
          Evaluator[Index].Value = x;

        //補助変数があれば代入します
        if(Auxiliary != null)
          foreach(var Item in Auxiliary)
            foreach(var Index in Evaluator.Select((m, i) => new { index = i, value = m }).Where(v => v.value.Type == TokenType.Variable & v.value.Key.ToLower() == Item.Name.ToLower()).Select(v => v.index))
              Evaluator[Index].Value = Item.Value;

        if(IsVerbose) {
          Console.WriteLine($@"{DateTime.Now} | 数式を評価します。 @MathExpression");
          Console.WriteLine($@"{DateTime.Now} | " + "\t" + $@"<{string.Join(",", Evaluator.Select(v => v.Value))}>");
        }
        //計算を実行します
        while(Evaluator.Count > 1) {//トークンが1個になるまで
          var Operator = Evaluator.Where(v => v.Type != TokenType.Numeric & v.Type != TokenType.Variable).First();//先頭の演算子または関数を探す
          var Index = Evaluator.IndexOf(Operator);
          double Result = double.NaN;
          if(Operator.Type == TokenType.Operator) {//四則演算なら
            switch(Operator.Value) {
              case "+":
                Result = Evaluator[Index - 2].Value + Evaluator[Index - 1].Value;
                break;
              case "-":
                Result = Evaluator[Index - 2].Value - Evaluator[Index - 1].Value;
                break;
              case "*":
                Result = Evaluator[Index - 2].Value * Evaluator[Index - 1].Value;
                break;
              case "/":
                try {
                  Result = Evaluator[Index - 2].Value / Evaluator[Index - 1].Value;
                } catch(Exception) { }
                break;
            }
          } else if(Operator.Type == TokenType.Function) {//関数なら
            switch(Operator.Value.ToLower()) {
              case "power":
              case "pow":
                Result = Math.Pow((double)Evaluator[Index - 2].Value, (double)Evaluator[Index - 1].Value);
                break;
              case "mod":
                Result = (double)Evaluator[Index - 1].Value != 0 ? (double)Evaluator[Index - 2].Value % (double)Evaluator[Index - 1].Value : double.NaN;
                break;
              case "exp":
                Result = Math.Exp((double)Evaluator[Index - 1].Value);
                break;
              case "log":
                Result = (double)Evaluator[Index - 1].Value > 0 ? Math.Log10((double)Evaluator[Index - 1].Value) : double.NaN;
                break;
              case "ln":
                Result = (double)Evaluator[Index - 1].Value > 0 ? Math.Log((double)Evaluator[Index - 1].Value) : double.NaN;
                break;
              case "sqrt":
                Result = (double)Evaluator[Index - 1].Value >= 0 ? Math.Sqrt((double)Evaluator[Index - 1].Value) : double.NaN;
                break;
              case "abs":
                Result = Math.Abs((double)Evaluator[Index - 1].Value);
                break;
              case "sin":
                Result = Math.Sin((double)Evaluator[Index - 1].Value);
                break;
              case "cos":
                Result = Math.Cos((double)Evaluator[Index - 1].Value);
                break;
              case "tan":
                Result = Math.Tan((double)Evaluator[Index - 1].Value);
                break;
              case "asin":
                Result = Math.Asin((double)Evaluator[Index - 1].Value);
                break;
              case "acos":
                Result = Math.Acos((double)Evaluator[Index - 1].Value);
                break;
              case "atan":
                Result = Math.Atan((double)Evaluator[Index - 1].Value);
                break;
              case "sinh":
                Result = Math.Sinh((double)Evaluator[Index - 1].Value);
                break;
              case "cosh":
                Result = Math.Cosh((double)Evaluator[Index - 1].Value);
                break;
              case "tanh":
                Result = Math.Tanh((double)Evaluator[Index - 1].Value);
                break;
              case "truncate":
                Result = Math.Truncate((double)Evaluator[Index - 1].Value);
                break;
              case "sign":
                Result = Math.Sign((double)Evaluator[Index - 1].Value);
                break;
              case "floor":
                Result = Math.Floor((double)Evaluator[Index - 1].Value);
                break;
              case "ceiling":
                Result = Math.Ceiling((double)Evaluator[Index - 1].Value);
                break;
              case "round":
                Result = Math.Round((double)Evaluator[Index - 1].Value);
                break;
              //以下は特殊処理
              case "sinc":
                try { Result = Math.Sin((double)Evaluator[Index - 1].Value) / (double)Evaluator[Index - 1].Value; } catch(Exception) { Result = 1; }
                break;
              case "decay":
                a = (double)Evaluator[Index - 2].Value;
                b = (double)Evaluator[Index - 1].Value;
                Result = a * Math.Exp(-(double)Evaluator[Index - 3].Value / b);
                break;
              case "stretched":
                a = (double)Evaluator[Index - 3].Value;
                b = (double)Evaluator[Index - 2].Value;
                c = (double)Evaluator[Index - 1].Value;
                Result = a * Math.Exp(-Math.Pow((double)Evaluator[Index - 4].Value, c) / b);
                break;
              case "gauss":
                a = (double)Evaluator[Index - 3].Value;
                b = (double)Evaluator[Index - 2].Value;
                c = (double)Evaluator[Index - 1].Value;
                Result = a / (Math.Sqrt(2 * Math.PI) * c) * Math.Exp(-Math.Pow(((double)Evaluator[Index - 4].Value - b) / c, 2) / 2);
                break;
              case "lorentz":
                a = (double)Evaluator[Index - 3].Value;
                b = (double)Evaluator[Index - 2].Value;
                c = (double)Evaluator[Index - 1].Value;
                Result = a / Math.PI * c / (Math.Pow((double)Evaluator[Index - 4].Value - b, 2) + Math.Pow(c, 2));
                break;
              case "foigt":
                a = (double)Evaluator[Index - 4].Value;
                b = (double)Evaluator[Index - 3].Value;
                c = (double)Evaluator[Index - 2].Value;
                d = (double)Evaluator[Index - 1].Value;
                Result = a * (d / (Math.Sqrt(2 * Math.PI) * c) * Math.Exp(-Math.Pow(((double)Evaluator[Index - 5].Value - b) / c, 2) / 2) + //Gauss part
                              (1 - d) / Math.PI * c / (Math.Pow((double)Evaluator[Index - 5].Value - b, 2) + Math.Pow(c, 2))); //Lorentz part
                break;
            }
          }
          Processed = new Token(TokenType.Numeric, Result);
          for(int Jndex = 0; Jndex <= Operator.NumberOfParameter; Jndex++)
            Evaluator.RemoveAt(Index - Operator.NumberOfParameter);
          Evaluator.Insert(Index - Operator.NumberOfParameter, Processed);
          if(IsVerbose)
            Console.WriteLine($@"{DateTime.Now} | " + "\t" + $@"<{string.Join(",", Evaluator.Select(v => v.Value))}>");
        }
        if(IsVerbose)
          Console.WriteLine($@"{DateTime.Now} | 結果は {Evaluator[0].Value} です。 @MathExpression");
        return Evaluator[0].Value;
      } else
        return double.NaN;
    }

    //------------------------------
    /// <summary>
    /// 中置記法で再構成した数式を取得します。
    /// </summary>
    /// <returns>再構成した数式を文字列で返します。</returns>
    public string Aggregated {
      get {
        if(Tokens.Count > 0) {
          List<Token> Aggregated = new List<Token>(Tokens);

          while(Aggregated.Count > 1) {
            var Operator = Aggregated.Where(v => v.Type != TokenType.Numeric & v.Type != TokenType.Variable).First();
            var Index = Aggregated.IndexOf(Operator);
            Token Processed = null;
            if(Operator.Type == TokenType.Operator)
              Processed = new Token(TokenType.Numeric, $@"({Aggregated[Index - 2].Key}{Operator.Key}{Aggregated[Index - 1].Key})");
            else if(Operator.Type == TokenType.Function)
              Processed = new Token(TokenType.Numeric, $@"{Operator.Key}({string.Join(",", Aggregated.Skip(Index - Operator.NumberOfParameter).Take(Operator.NumberOfParameter).Select(v => v.Key))})");
            for(int Jndex = 0; Jndex <= Operator.NumberOfParameter; Jndex++)
              Aggregated.RemoveAt(Index - Operator.NumberOfParameter);
            Aggregated.Insert(Index - Operator.NumberOfParameter, Processed);
          }
          return Aggregated[0].Value;
        } else
          return "";
      }
    }

    //------------------------------
    /// <summary>
    /// 後置記法で再構成した数式を取得します。
    /// </summary>
    /// <returns>再構成した数式を文字列で返します。</returns>
    public string Tokenized {
      get => string.Join(",", Tokens.Select(t => t.Key));
    }

    #endregion
  }//end of class MathExpression

  //====================================================================================
  /// <summary>
  /// 文字列で与えられた論理式を評価します。後置記法への変換は<a href="http://7ujm.net/etc/calcstart.html">このページ</a>を参照して構築しました。
  /// </summary>
  /// <example>
  /// LogicExpressionの使用例を示します。評価式の評価は<see cref="Validation"/>で行います。
  /// <code>
  /// void TestLogicExpression() {
  ///   string Logic = "TickTimer&gt;5 &amp; TurboRotation=true";//論理式を設定します。二つの評価式が入っています。空白は除去されます。
  ///   RadonLab.Formula.LogicExpression Formula = new RadonLab.Formula.LogicExpression(Logic);//LogicExpressionを初期化します。
  ///   
  ///   if(Formula != null) {
  ///     Console.WriteLine($@"{Formula.Expression}");//入力された論理式を出力します。
  ///     Console.WriteLine($@"{Formula.Tokenized}");//後置記法で論理式を出力します。TickTimer&gt;5,TurboRotation=true,&amp;
  ///     Console.WriteLine($@"{Formula.Aggregated}");//中置記法に再構成した論理式を出力します。(TickTimer&gt;5&amp;TurboRotation=true)
  ///   
  ///     bool Result = Formula.Evaluate(new Dictionary&lt;string, bool&gt;() { { "TickTimer&gt;5", true } })}");//二つの評価式の値を指定して論理式を評価します。この場合、TickTimer&gt;5のみtrueなので、結果はfalseです。
  ///     Result = Formula.Evaluate(new Dictionary&lt;string, bool&gt;() { { "TickTimer&gt;5", true }, { "TurboRotation=true", true } })}");//両方の評価式がtrueなので、結果はtrueです。
  ///   }
  /// }
  /// </code>
  /// </example>
  public class LogicExpression {
    #region フィールド

    //------------------------------
    /// <summary>
    /// 入力された式を取得します。
    /// </summary>
    public string Expression { get; private set; }

    //------------------------------
    /// <summary>
    /// 詳細モードを取得または設定します。
    /// </summary>
    public bool IsVerbose { get; set; }

    //------------------------------
    /// <summary>
    /// 式の構成要素リストを取得します。
    /// </summary>
    public List<Token> Tokens { get; private set; }

    #endregion

    #region メソッド

    //------------------------------
    /// <summary>
    /// LogicExpressionを初期化します。
    /// </summary>
    /// <param name="Expression">論理式を設定します。</param>
    /// <param name="IsVerbose">詳細モードを設定します。</param>
    public LogicExpression(string Expression, bool IsVerbose = false) {
      this.IsVerbose = IsVerbose;
      this.Expression = Regex.Replace(Expression, @"\s", "");//空白を削除する
      Tokens = Activate(Tokenize(Evacuate(this.Expression, out var Constants)), Constants);//式をトークン化する
    }

    //------------------------------
    /// <summary>
    /// 式中の比較式をタグ[\d]で退避します。
    /// </summary>
    /// <param name="Expression">論理式を指定します。</param>
    /// <param name="Constants">構成要素リストを指定します。</param>
    /// <returns>比較式を置換した後の論理式を文字列で返します。</returns>
    string Evacuate(string Expression, out List<dynamic> Constants) {
      const string Logic = @"(?<logic>(?<left>[\w:]*)?(?<operator>[!=\<\>]{0,2})\b(?<right>[\w\.]+))";//比較式

      if(IsVerbose)
        Console.WriteLine($@"{DateTime.Now} | 論理式 {Expression} の評価式をタグ[\d]で置換します。 @LogicExpression");
      var Values = Regex.Matches(Expression, $@"({Logic})");
      Constants = new List<dynamic>(Values.Cast<Match>().Select(m => m.Value));//論理式を取りだしてリストに格納する
      if(IsVerbose)
        Console.WriteLine($@"{DateTime.Now} | 捕捉した評価式は ""{string.Join(",", Constants)}"" です。 @LogicExpression");
      Expression = Regex.Replace(Expression, $@"({Logic})", new MatchEvaluator(m => $@"[{m.Index}]"));//数値に対応する部分を[適合位置]で置き換える。MatchEvaluaterのデリゲートでグループ番号が得られないための処置
      for(int Index = 0; Index < Constants.Count; Index++) //見つかった論理式ごとに
        Expression = Regex.Replace(Expression, $@"\[{Values[Index].Index}\]", $@"[{Index}]");//[適合位置]を[フィールド番号]で置き換える
      if(IsVerbose)
        Console.WriteLine($@"{DateTime.Now} | タグ置換した式は {Expression} です。 @LogicExpression");

      return Expression;
    }

    //------------------------------
    /// <summary>
    /// 式中の比較式がタグ[\d]で置換された論理式を構成要素に分解します。
    /// </summary>
    /// <param name="Expression">比較式がタグ化された論理式を指定します。</param>
    /// <param name="Depth">詳細モードの時に使用するインデントの深さを指定します。</param>
    /// <returns>構成要素リストを返します。</returns>
    List<Token> Tokenize(string Expression, int Depth = 0) {
      const string Constant = @"(?<constant>\[\d+\])";//置換された比較式
      const string Operator = @"(?<operator>[&\^\|]{1})";//論理演算の演算子
      const string Brackets = @"(?<leftbracket>\({1})";//括弧

      string Indents = new string('\t', Depth);//詳細モード用インデント

      List<Token> Tokenized = new List<Token>();//後置されたトークンリスト

      Regex Parser = new Regex($@"({Constant}|{Operator}|{Brackets}|)", RegexOptions.IgnoreCase);//正規表現パーサ
      List<string> Stack = new List<string>();//後置処理用のスタック
      int BracketPosition, BracketCount;//括弧を検索する変数
      string EnclosedArgument;//括弧に囲まれた式を格納するバッファ

      if(IsVerbose)
        Console.WriteLine($@"{DateTime.Now} | {Indents}論理式 {Expression} を後置処理します。");
      while(0 < Expression.Length) {//式がすべて処理されるまで続ける
        var Matched = Parser.Match(Expression);//文字列の先頭から要素を検索し
        if(Matched.Success) {//要素が見つかれば
          var Captures = Matched.Groups.Cast<Group>().Where(m => m.Success == true & !int.TryParse(m.Name, out int r));//マッチコレクションの要素のなかで、Successがtureで名前が数値でないもの、を取り出す。
          if(Captures.Count() == 1) {//念のため、キャプチャ数が1であることを確かめる
            var Captured = Captures.First();//先頭がキャプチャ情報になっている
            if(IsVerbose)
              Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}""{Captured.Value}""が捕捉されました。");
            switch(Captured.Name) {//キャプチャ名によって処理を分岐する
                                   //-----------------------------------------------
              case "constant"://数値なら
                Tokenized.Add(new Token(TokenType.Logic, Captured.Value));//トークンリストに出力する
                Expression = Expression.Substring(Matched.Value.Length);//キャプチャ定数を元の式から削除
                break;
              //-----------------------------------------------
              case "operator":
                if(Stack.Count == 0)//スタックが空なら積む
                  Stack.Add(Captured.Value);
                else {//スタックが空でないなら
                  while(Stack.Count > 0 && Priority(Captured.Value) <= Priority(Stack.First())) { //キャプチャ演算子の優先度がスタック先頭と同じか低い間は
                    Tokenized.Add(new Token(TokenType.Operator, Stack.First(), NumberOfParameter: 2));//スタックから取りだしてトークンリストに追加する
                    Stack.RemoveAt(0);//スタックの先頭を削除する
                  }
                  Stack.Insert(0, Captured.Value);//キャプチャ演算子をスタックに積む
                }
                Expression = Expression.Substring(Matched.Value.Length);//キャプチャ演算子を元の式から削除
                break;
              //-----------------------------------------------
              case "leftbracket"://関数に属さない独立した開く括弧なら
                BracketPosition = 0;//確固位置
                BracketCount = 0;//括弧数
                while(true) {
                  if(Expression[BracketPosition] == '(')//左括弧なら
                    BracketCount++;//括弧数を増やす
                  if(Expression[BracketPosition] == ')')//右括弧なら
                    BracketCount--;//括弧数を減らす
                  if(BracketCount == 0)//括弧数がゼロになったら
                    break;//対応した括弧の位置に到達
                  BracketPosition++;//位置をずらす
                }
                if(IsVerbose)
                  Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}対応する"")""が捕捉されました。");
                EnclosedArgument = Expression.Substring(1, BracketPosition - 1);//括弧の中の式を取り出して
                Expression = Expression.Substring(BracketPosition + 1);//対応括弧までの範囲を元の式から削る
                foreach(var Item in Tokenize(EnclosedArgument, Depth + 1))//括弧内の式をトークン化して
                  Tokenized.Add(Item);//トークン列に加える
                break;
            }
          }
        }
      }

      //式がすべて処理されたら
      foreach(var Item in Stack)//スタックに残っている演算子を
        Tokenized.Add(new Token(TokenType.Operator, Item, NumberOfParameter: 2));//トークンリストに追加する
      if(IsVerbose)
        Console.WriteLine($@"{DateTime.Now} | {Indents + '\t'}後置処理された結果は {string.Join(",", Tokenized.Select(v => v.Value))} です。");

      return Tokenized;
    }

    //------------------------------
    /// <summary>
    /// タグ[\d]で退避した比較式を構成要素リストに戻します。
    /// </summary>
    /// <param name="Tokenized">構成要素リストを指定します。</param>
    /// <param name="Constants">定数リストを指定します。</param>
    /// <returns>構成要素リストを返します。</returns>
    List<Token> Activate(List<Token> Tokenized, List<dynamic> Constants) {
      for(int Index = 0; Index < Tokenized.Count; Index++) {
        if(Tokenized[Index].Type == TokenType.Logic) {
          int Jndex = int.Parse(Regex.Replace(Tokenized[Index].Value, @"[\[\]]", ""));
          Tokenized[Index].Key = Constants[Jndex];
          Tokenized[Index].Value = false;
          //if(IsVerbose)
          //  Console.WriteLine($@"{DateTime.Now} | ({Tokenized[Index].Key}, {Tokenized[Index].Value}) です。");
        }
      }
      return Tokenized;
    }

    //------------------------------
    /// <summary>
    /// 演算子の優先度を返します。
    /// </summary>
    /// <param name="Operator">演算子を指定します。</param>
    /// <returns>優先度を返します。</returns>
    int Priority(string Operator) {
      switch(Operator) {
        case "|"://OR
          return 0;
        case "^"://XOR
          return 1;
        case "&"://AND
          return 2;
        case "!"://NOT
          return 3;
        default://それ以外
          return 4;
      }
    }

    //------------------------------
    /// <summary>
    /// 論理式を評価します。
    /// </summary>
    /// <param name="NewStates">論理変数の名前と値をDictionaryで指定します</param>
    /// <returns>論理式の状態を返します。</returns>
    public bool Evaluate(Dictionary<string, bool> NewStates) {
      if(Tokens.Count > 0) {
        List<Token> Evaluator;
        Token Processed;

        foreach(var Item in NewStates)//入力された情報を
          Tokens.Where(v => v.Key == Item.Key).First().Value = Item.Value;//状態変数にセットする
        Evaluator = new List<Token>(Tokens);

        if(IsVerbose) {
          Console.WriteLine($@"{DateTime.Now} | 論理式を評価します。 @LogicExpression");
          Console.WriteLine($@"{DateTime.Now} | " + "\t" + $@"<{string.Join(",", NewStates.Select(v => $@"""{v.Key}""={v.Value}"))}>");
        }

        while(Evaluator.Count > 1) {//トークンが1個になるまで
          var Operator = Evaluator.Where(v => v.Type != TokenType.Logic).First();//先頭の演算子を探す
          var Index = Evaluator.IndexOf(Operator);
          bool Result = false;
          if(Operator.Type == TokenType.Operator) {//論理演算なら
            switch(Operator.Value) {
              case "|":
                Result = Evaluator[Index - 2].Value | Evaluator[Index - 1].Value;
                break;
              case "^":
                Result = Evaluator[Index - 2].Value ^ Evaluator[Index - 1].Value;
                break;
              case "&":
                Result = Evaluator[Index - 2].Value & Evaluator[Index - 1].Value;
                break;
            }
            Processed = new Token(TokenType.Logic, Result, "Evaluated");
            for(int Jndex = 0; Jndex <= Operator.NumberOfParameter; Jndex++)
              Evaluator.RemoveAt(Index - Operator.NumberOfParameter);
            Evaluator.Insert(Index - Operator.NumberOfParameter, Processed);
          }
          if(IsVerbose)
            Console.WriteLine($@"{DateTime.Now} | " + "\t" + $@"<{string.Join(",", Evaluator.Select(v => v.Value))}>");
        }
        if(IsVerbose)
          Console.WriteLine($@"{DateTime.Now} | 結果は {Evaluator[0].Value} です。 @LogicExpression");
        return Evaluator[0].Value;
      } else
        return false;
    }

    //------------------------------
    /// <summary>
    /// 中置記法で再構成した論理式を取得します。
    /// </summary>
    /// <returns>再構成した論理式を文字列で返します。</returns>
    public string Aggregated {
      get {
        if(Tokens.Count > 0) {
          List<Token> Aggregated = new List<Token>(Tokens);

          while(Aggregated.Count > 1) {
            var Operator = Aggregated.Where(v => v.Type != TokenType.Logic).First();
            var Index = Aggregated.IndexOf(Operator);
            Token Processed = new Token(TokenType.Logic, $@"({Aggregated[Index - 2].Key}{Operator.Key}{Aggregated[Index - 1].Key})");
            for(int Jndex = 0; Jndex <= Operator.NumberOfParameter; Jndex++)
              Aggregated.RemoveAt(Index - Operator.NumberOfParameter);
            Aggregated.Insert(Index - Operator.NumberOfParameter, Processed);
          }
          return Aggregated[0].Value;
        } else
          return "";
      }
    }

    //------------------------------
    /// <summary>
    /// 後置記法で再構成した論理式を取得します。
    /// </summary>
    /// <returns>再構成した論理式を文字列で返します。</returns>
    public string Tokenized {
      get => string.Join(",", Tokens.Select(t => t.Key));
    }

    #endregion
  }//end of class LogicExpression

}//end of namespace Expression
