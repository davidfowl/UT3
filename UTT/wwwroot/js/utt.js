(function (name) {
    Vue.config.devtools = true

    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/utt')
        .configureLogging(signalR.LogLevel.Trace)
        .build();

    var app = new Vue({
        el: '#app',
        data: {
            name: name,
            games: [],
            game: null,
            newGame: '',
            users: [],
            messages: [],
            message: ''
        },
        methods: {
            getStatus: function (game) {
                switch (game.status) {
                    case 0:
                        return 'Waiting';
                    case 1:
                        return 'Playing';
                    case 2:
                        return 'Ended';
                }
            },
            addChatMessage: function () {
                connection.invoke('sendToLobby', this.message);
                this.message = '';
            },
            startGame: function () {
                if (this.newGame) {
                    connection.invoke('createGame', this.newGame);
                    this.newGame = '';
                }
            },
            selectGame: function (game) {
                this.game = game;
            },
            joinGame: function (id) {
                connection.invoke('joinGame', id);
            },
            playCell: function (outerRowIndex, outerColIndex, innerRowIndex, innerColIndex) {
                if (this.game.playerTurn !== this.name) {
                    // Not this player's turn
                    return;
                }

                var board = this.game.board.boards[outerRowIndex][outerColIndex];
                var nextPos = this.game.nextBoardPosition;
                var expectedBoard = nextPos.row === -1 ? null : this.game.board.boards[nextPos.row][nextPos.column];

                if (expectedBoard != null && expectedBoard != board) {
                    // Played in the wrong board
                    return;
                }

                // Only send to the server if it's this player's turn and there's nothing in this slot
                var cell = board.cells[innerRowIndex][innerColIndex];
                if (cell !== 0) {
                    // Spot taken
                    return;
                }
                connection.invoke('playTurn', this.game.id, outerRowIndex, outerColIndex, innerRowIndex, innerColIndex);
            }
        }
    });

    connection.on('usersChanged', function (users) {
        app.users = users;
    });

    connection.on('lobbyMessage', function (from, message) {
        app.messages.push({ from: from, text: message });
    });

    connection.on('gameUpdated', function (games) {
        app.games = games;

        // Select the first active game
        for (var i = 0; i < app.games.length; ++i) {
            var game = app.games[i];
            if (game.status == 1) {
                app.selectGame(game);
                break;
            }
        }
    });

    connection.on('playMove', function (id, nextMove) {
        for (var i = 0; i < app.games.length; ++i) {
            var game = app.games[i];
            if (game.id == id) {
                game.playerTurn = nextMove.playerTurn;
                game.nextBoardPosition = nextMove.nextBoardPosition;
                var outerRowIndex = nextMove.outerRowIndex;
                var outerColIndex = nextMove.outerColIndex;
                var innerRowIndex = nextMove.innerRowIndex;
                var innerColIndex = nextMove.innerColIndex;
                var value = nextMove.cellValue;

                var board = game.board.boards[outerRowIndex][outerColIndex];
                board.winner = nextMove.boardWinner;
                game.winner = nextMove.winner;

                if (game.winner) {
                    // Game over
                    game.status = 2;
                }

                Vue.set(board.cells[innerRowIndex], innerColIndex, value);
                break;
            }
        }
    });

    connection.start();
})(userName);