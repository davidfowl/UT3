var connection = new signalR.HubConnectionBuilder()
    .withUrl('/utt')
    .configureLogging(signalR.LogLevel.Trace)
    .build();

var app = new Vue({
    el: '#app',
    data: {
        name: '',
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

    if (games.length > 0) {
        // Temporary to select a game
        app.selectGame(games[0]);
    }    
});

connection.on('playMove', function (id, outerRowIndex, outerColIndex, innerRowIndex, innerColIndex, value) {
    // TODO: Support multiple games in parallel
    if (app.game && app.game.id == id)
    {
        Vue.set(app.game.board.boards[outerRowIndex][outerColIndex].cells[innerRowIndex], innerColIndex, value);
    }    
});    

connection.start();