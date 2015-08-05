'use strict';

angular.module('spa2App')
  .controller('MainCtrl', function ($scope) {
  	$scope.foo = '123';
    $scope.awesomeThings = [
      'HTML5 Boilerplate',
      'AngularJS',
      'Karma',
      'Joseph'
    ];
    $scope.isPage = function (id) {
    	return document.location.hash.indexOf(id) > -1
    }
  });
