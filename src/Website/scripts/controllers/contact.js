angular.module('spa2App').controller('ContactCtrl', ['$scope', '$http', 'Config', function($scope, $http, Config) {
	$scope.contact = {
		fullname: '',
		email: '',
		message: ''
	};
	$scope.send = function (model) {
		console.log(model, Config);
		$http.post(Config.url + '/contact', model)
			.then(function (response) {
				console.log(response); // success
			}, function (response) {
				console.log(response); // fail
			});
	}
}]);