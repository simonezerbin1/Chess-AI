namespace Chess.Game {
	using System.Threading.Tasks;
	using System.Threading;

	public class AIPlayer : Player {



		Search search;
		AISettings settings;
		bool moveFound;
		Move move;
		Board board;
		CancellationTokenSource cancelSearchTimer;



		public AIPlayer (Board board, AISettings settings) {
			this.settings = settings;
			this.board = board;
			settings.requestAbortSearch += TimeOutThreadedSearch;
			search = new Search (board, settings);
			search.onSearchComplete += OnSearchComplete;
			search.searchDiagnostics = new Search.SearchDiagnostics ();

		}

		public override void Update () {
			if (moveFound) {
				moveFound = false;
				ChoseMove (move);
			}

			settings.diagnostics = search.searchDiagnostics;

		}

		public override void NotifyTurnToMove () {

			

			
				if (settings.useThreading) {
					StartThreadedSearch ();
				} else {
					StartSearch ();
				}
			
		}

		void StartSearch () {
			search.StartSearch ();
			moveFound = true;
		}

		void StartThreadedSearch () {
			//Thread thread = new Thread (new ThreadStart (search.StartSearch));
			//thread.Start ();
			Task.Factory.StartNew (() => search.StartSearch (), TaskCreationOptions.LongRunning);

			if (!settings.endlessSearchMode) {
				cancelSearchTimer = new CancellationTokenSource ();
				Task.Delay (settings.searchTimeMillis, cancelSearchTimer.Token).ContinueWith ((t) => TimeOutThreadedSearch ());
			}

		}

		// Note: called outside of Unity main thread
		void TimeOutThreadedSearch () {
			if (cancelSearchTimer == null || !cancelSearchTimer.IsCancellationRequested) {
				search.EndSearch ();
			}
		}

		/*void PlayBookMove(Move bookMove) {
			this.move = bookMove;
			moveFound = true;
		}*/

		void OnSearchComplete (Move move) {
			// Cancel search timer in case search finished before timer ran out (can happen when a mate is found)
			cancelSearchTimer?.Cancel ();
			moveFound = true;
			this.move = move;
		}
	}
}