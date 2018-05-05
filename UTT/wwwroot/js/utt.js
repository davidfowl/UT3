(function (name) {
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
                // Only send to the server if it's this player's turn and there's nothing in this slot
                var cell = this.game.board.boards[outerRowIndex][outerColIndex].cells[innerRowIndex][innerColIndex];
                if (this.game.playerTurn == this.name && cell === 0) {
                    connection.invoke('playTurn', this.game.id, outerRowIndex, outerColIndex, innerRowIndex, innerColIndex);
                }
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

    connection.on('playMove', function (id, outerRowIndex, outerColIndex, innerRowIndex, innerColIndex, value, playerTurn) {
        for (var i = 0; i < app.games.length; ++i) {
            var game = app.games[i];
            if (game.id == id) {
                game.playerTurn = playerTurn;
                game.nextBoardPosition = { row: innerRowIndex, column: innerColIndex };
                Vue.set(game.board.boards[outerRowIndex][outerColIndex].cells[innerRowIndex], innerColIndex, value);
                break;
            }
        }
    });

    connection.start();
})(userName);