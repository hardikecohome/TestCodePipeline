/*!
 * layout-seed Gruntfile
 * @author Petrus Eugene
 */

'use strict';

module.exports = function (grunt) {

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        sass_import: {
            options: {
                basePath: 'src/styles/'
            },
            dist: {
                files: {
                    'application.scss': ['modules/*', 'desktop/*'],
                    'application-mobile.scss': ['modules/*', 'mobile/*'],
                    'application-tablet.scss': ['modules/*', 'tablet/*']
                }
            }
        },
        sass: {
            dev: {
                options: {
                    compass: false
                },
                files: {
                    '../css/styles.css': 'src/styles/application.{scss,sass}',
                    '../css/styles-tablet.css': 'src/styles/application-tablet.{scss,sass}',
                    '../css/styles-mobile.css': 'src/styles/application-mobile.{scss,sass}'
                }
            }
        },
        copy: {
            main: {
                files: [
                    // includes files within path
                    {
                        expand: true,
                        cwd: 'src/img',
                        src: '**',
                        dest: '../images',
                        filter: 'isFile'
                    },
                    {
                        expand: true,
                        cwd: 'src/fonts',
                        src: '**',
                        dest: '../fonts',
                        filter: 'isFile'
                    },
                    {
                        expand: true,
                        cwd: 'src/librarys/**/*.js',
                        src: ['**'],
                        dest: '../../Scripts/vendor'
                    },
                    {
                        expand: true,
                        cwd: 'src/librarys/**/*.css',
                        src: ['**'],
                        dest: '../../Scripts/css'
                    },
                ],
            },
        },
        ejs: {
            all: {
                src: ['**/*.ejs', '!_partials/**/*', '!_layouts/**/*'],
                cwd: 'src/',
                dest: 'dist/',
                expand: true,
                ext: '.html',
                options: {
                    title: 'Default title',
                    keywords: 'Keywords of the page',
                    description: 'Description of the page'
                }
            }
        },
        includeSource: {
            options: {
                basePath: 'dist/',
                baseUrl: '',
                typeMappings: {
                    'sass': 'scss'
                },
                templates: {
                    html: {
                        js: '<script src="{filePath}"></script>',
                        css: '<link rel="stylesheet" type="text/css" href="{filePath}" />',
                    },
                    scss: {
                        scss: '@import "{filePath}";',
                        css: '@import "{filePath}";',
                    }
                }
            },
            myTarget: {
                files: [{
                        expand: true,
                        cwd: 'dist/',
                        src: ['*.html'],
                        dest: 'dist/',
                        ext: '.html',
                        extDot: 'first'
                    },
                    {
                        expand: true,
                        cwd: 'src/styles',
                        src: ['*.sass'],
                        dest: 'src/styles',
                        ext: '.sass',
                        extDot: 'first'
                    }
                ]
            }
        },
        prettify: {
            options: {
                "indent": 2,
                "wrap_line_length": 90,
                "unformatted": ["a", "pre", "code"]
            },
            all: {
                expand: true,
                cwd: 'dist/',
                ext: '.html',
                src: ['*.html'],
                dest: 'dist/'
            }
        },
        watch: {
            options: {
                livereload: true,
            },
            scss: {
                files: 'src/styles/{,*/}*.{scss,sass}',
                tasks: ['sass_import', 'sass:dev']
            },
            js: {
                files: 'src/js/{,*/}*.js',
                tasks: ['concat:js']
            },
            ejs: {
                files: 'src/{,*/}*.ejs',
                tasks: ['ejs:all', 'includeSource', 'prettify'],
            }
        },
        concat: {
            js: {
                src: ['src/js/{,*/}*.js'],
                dest: 'dist/js/main.js'
            }
        }
    });

    require('matchdep').filterDev('grunt-*').forEach(grunt.loadNpmTasks);

    grunt.registerTask('default', [
        'sass:dev',
        'sass_import',
        'ejs:all',
        'copy:main',
        'includeSource',
        'prettify',
        'concat',
        'watch'
    ]);

    grunt.registerTask('build', [
        'sass_import',
        'sass:dev',
        'ejs:all',
        'copy:main',
        'includeSource',
        'prettify',
        'concat'
    ]);

    grunt.loadNpmTasks('grunt-include-source');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-prettify');
    grunt.loadNpmTasks('grunt-sass-import');
    grunt.loadNpmTasks('grunt-contrib-concat');
};
