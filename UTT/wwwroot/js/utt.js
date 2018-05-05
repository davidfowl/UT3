var connection = new signalR.HubConnectionBuilder()
    .withUrl('/utt')
    .configureLogging(signalR.LogLevel.Trace)
    .build();

var app = new Vue({
    el: '#app',
    data: {
        games: [],
        game: null,
        game2: {
            board: {
                cells: [
                    [[[0, 1, 2], [0, 1, 0], [2, 0, 0]],
                    [[0, 1, 2], [0, 1, 0], [2, 0, 0]],
                    [[0, 1, 2], [0, 1, 0], [2, 0, 0]]],

                    [[[0, 1, 2], [0, 1, 0], [2, 0, 0]],
                    [[0, 1, 2], [0, 1, 0], [2, 0, 0]],
                    [[0, 1, 2], [0, 1, 0], [2, 0, 0]]],

                    [[[0, 1, 2], [0, 1, 0], [2, 0, 0]],
                    [[0, 1, 2], [0, 1, 0], [2, 0, 0]],
                    [[0, 1, 2], [0, 1, 0], [2, 0, 0]]]
                ]
            }
        },
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
        select: function (game) {
            this.game = game;
            connection.invoke('selectGame', game.id);
        },
        joinGame: function (id) {
            connection.invoke('joinGame', id);
        },
        playCell: function (outerRowIndex, outerColIndex, innerRowIndex, innerColIndex) {
            connection.invoke('playTurn', this.game.id, outerRowIndex, outerColIndex, innerRowIndex, innerColIndex);
            // Vue.set(gameRow, index, val == 0 ? 1 : (val == 1 ? 2 : 1));
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
});

connection.start();