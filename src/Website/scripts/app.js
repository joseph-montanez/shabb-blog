'use strict';

var prefix = 'app';

angular.module('spa2App', [
  // 'ngCookies',
  'ngResource',
  'ngSanitize',
  'ngRoute',
  'gist'
])
  .config(['$routeProvider', '$locationProvider', '$httpProvider', function ($routeProvider, $locationProvider, $httpProvider) {
    $locationProvider.html5Mode(false).hashPrefix('!');
    $httpProvider.defaults.useXDomain = true;
    delete $httpProvider.defaults.headers.common['X-Requested-With'];
    //$locationProvider.html5Mode(true);
    $routeProvider
      .when('/', {
        templateUrl: 'views/main.html',
        controller: 'MainCtrl'
      })
      .when('/about', {
        templateUrl: 'views/about.html',
        controller: 'MainCtrl'
      })
      .when('/blog', {
        templateUrl: 'views/blog.html',
        controller: 'BlogListCtrl'
      })
      .when('/contact', {
        templateUrl: 'views/contact.html',
        controller: 'ContactCtrl'
      })
      .when('/blog/page/:PageNo', {
        templateUrl: 'views/blog.html',
        controller: 'BlogListCtrl'
      })
      .when('/blog/entry/:URL/:EntryID', {
        templateUrl: 'views/blog-entry.html',
        controller: 'BlogEntryCtrl'
      })
      .when('/work', {
        templateUrl: 'views/work.html',
        controller: 'MainCtrl'
      })
      .otherwise({
        redirectTo: '/'
      });
  }]);
