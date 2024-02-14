# TinyChessDuck: A 969 Tokens C# Chess Bot

- This is a chess bot I wrote in a single 969-tokens C# file as part of youtuber [Sebastian Lague](https://www.youtube.com/@SebastianLague)'s [Chess Coding Challenge](https://www.youtube.com/watch?v=iScy18pVR58).
- The challenge was: Given a [chess framework](https://github.com/SebLague/Chess-Challenge), implement a chess AI under the limit of 1024 tokens only to battle it out against other participants' bots.
- Overall, I enjoyed participating very much, I learned a lot about chess programming, search algorithms and evaluation heuristics, and my bot did surprisingly well in the tournament (see [results](https://github.com/m0hossam/tiny-chess-duck#results)).

## Table of Contents

- [Objective](#objective)
- [The AI](#the-ai)
- [Results](#results)
- [Try It](#try-it)
- [Credits](#credits)

## Objective

The objective of this challenge was to implement strong chess search and evaluation algorithms in a maximum of 1024 tokens of code, without the overhead of writing an entire chess framework. This limit was (probably) enforced for 2 reasons:
- Preventing participants from copying code from famous open source chess engines (for example: [Stockfish](https://github.com/official-stockfish/Stockfish))
- Encouraging participants to come up with creative ways of writing tiny chess AI algorithms, for example:
  - Designing compression/encoding algorithms for big amounts of values like [piece square tables](https://www.chessprogramming.org/Piece-Square_Tables)
  - Choosing between multiple search extension algorithms, pruning techniques and evaluation parameters (you can't fit'em all in 1024 tokens!)

I learned a lot about chess AI programming and I got a chance to apply my problem-solving techniques to overcome the token-limit problem, like: 
- Using bit-manipulation to compress [piece square tables](https://www.chessprogramming.org/Piece-Square_Tables) (768 tokens) into 64-bit `ulong`s (96 tokens)
- Using basic search algorithms like [minimax](https://www.chessprogramming.org/Minimax) and [alpha-beta pruning](https://www.chessprogramming.org/Alpha-Beta)
- Using dynamic programming techniques in [transposition tables](https://www.chessprogramming.org/Transposition_Table) to reduce search time

## The AI

Most chess bots consist of a **Search** function and an **Evaluation** function.
<br>
- My Search function:
  - **[Minimax](https://www.chessprogramming.org/Minimax) search and [alpha-beta pruning](https://www.chessprogramming.org/Alpha-Beta) inside a [negamax](https://www.chessprogramming.org/Negamax) framework**
  - **[Quiescence search](https://www.chessprogramming.org/Quiescence_Search)**

- My Evaluation function:
  - **[Piece square tables only evalution](https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function)**

- Techniques to reduce search time:
  - **[Most-valuable-victim - least-valuable-aggressor](https://www.chessprogramming.org/MVV-LVA) move ordering**
  - **[Iterative deepening framework](https://www.chessprogramming.org/Iterative_Deepening)**

## Results

My bot ranked **117th out of 624 bots**, sadly - but understandably - it wasn't mentioned in Sebastian's [results video](https://www.youtube.com/watch?v=Ne40a5LkK6A). Here is a screenshot from the video:
<br>
![Bot Rank](https://github.com/m0hossam/tiny-chess-duck/assets/115721045/490fbf55-6b29-41bf-9e95-8bbe51359aea)

Tournament games played by all bots were published to this [repo](https://github.com/SebLague/Tiny-Chess-Bot-Challenge-Results), I wrote a simple Python script to extract my bot's 64 games out of all games and analyze basic stats like wins/losses and color. Here are the results:
![bot results](https://github.com/m0hossam/tiny-chess-duck/assets/115721045/adb5304a-ac97-468b-9e6c-acc6a84ae9ea)

## Try It

- If you just want to play against the bot, download and run the executable from [here](https://github.com/m0hossam/tiny-chess-duck/releases/download/contest-version/TinyChessDuck.rar)
- If you want to run the project in your IDE, you must have `.NET 6.0`, download the source code and open `Chess-Challenge.sln`

## Credits

- [Sebastian Lague](https://www.youtube.com/@SebastianLague)
  - Organized the challenge, wrote the entire [chess framework](https://github.com/SebLague/Chess-Challenge) and its documentation
- [Chess coding discord community](https://github.com/SebLague/Chess-Challenge/discussions/156)
  - Helped with code and guides, especially users `selenaut` and `jw1912` for open-sourcing their code
- [Chess programming wiki](https://www.chessprogramming.org/Main_Page)
  - Very good hub of chess programming knowledge
- Yours truly
  - Wrote [`MyBot.cs`](https://github.com/m0hossam/tiny-chess-duck/blob/main/Chess-Challenge/src/My%20Bot/MyBot.cs) and [`PackingPST.cs`](https://github.com/m0hossam/tiny-chess-duck/blob/main/Chess-Challenge/src/My%20Bot/PackingPST.cs)
