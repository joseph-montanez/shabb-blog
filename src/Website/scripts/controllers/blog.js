'use strict';

angular.module('spa2App').factory('Config', function() {
		return {
			url: '//localhost:8083'
		}
	});

angular.module('spa2App').factory('BlogPages', ['$resource', 'Config',
	function($resource, Config){
		return $resource(Config.url + '/blog/posts/page/:PageNo', {}, {
			query: {method:'GET', params:{PageNo:'0'}}
		});
	}]);

angular.module('spa2App').factory('BlogEntry', ['$resource', 'Config',
	function($resource, Config){
		return $resource(Config.url + '/blog/posts/entry/:EntryID', {}, {
			query: {method:'GET', params:{EntryID:'0'}}
		});
	}]);

  // declare a new module, and inject the $compileProvider
angular.module('spa2App')
  .directive('compile', ['$compile', function ($compile) {
	  return function(scope, element, attrs) {
		  scope.$watch(
			function(scope) {
			   // watch the 'compile' expression for changes
			  return scope.$eval(attrs.compile);
			},
			function(value) {
			  // when the 'compile' expression changes
			  // assign it into the current DOM
			  element.html(value);

			  // compile the new DOM and link it to the current
			  // scope.
			  // NOTE: we only compile .childNodes so that
			  // we don't get into infinite loop compiling ourselves
			  $compile(element.contents())(scope);
			}
		);
	};
}]);

angular.module('spa2App').controller('BlogListCtrl', ['$scope', '$routeParams', 'BlogPages', function($scope, $routeParams, BlogPages) {
	var pageNo = $routeParams.PageNo || 0;
	$scope.PageNo = parseInt(pageNo);
	$scope.Blogs = BlogPages.query({PageNo: pageNo}, function ($resource) {
		for (var i = 0; i < $resource.Items.length; i++) {
			var dateTs = parseInt($resource.Items[i].PubDate.match(/[0-9-]+/)[0]);
			$resource.Items[i].PubDate = new Date(dateTs);
		}
	});
}]);
 
angular.module('spa2App').controller('BlogEntryCtrl', ['$scope', '$routeParams', '$compile', 'BlogEntry', function($scope, $routeParams, $compile, BlogEntry) {
	$scope.page = {};
	$scope.Blog = BlogEntry.get({EntryID: $routeParams.EntryID}, function ($resource) {
		console.log($resource);
		var dateTs = parseInt($resource.PubDate.match(/[0-9-]+/)[0]);
		$resource.PubDate = new Date(dateTs);
		$scope.title = $resource.Title;
		//$resource.Content = $compile($resource.Content)($scope);
		//$scope.$apply();
	});
}]);