var gulp = require('gulp');
var del = require('del');
var gts = require('gulp-typescript');
var browserify = require('browserify');
var vss = require('vinyl-source-stream');
var buffer = require('vinyl-buffer');
var run = require('run-sequence');
var fs = require('fs');
var path = require('path');

//change cwd to the dir current file belongs to
process.chdir(__dirname);

//folders
var OUT_DIR = process.env.JSOBJ;
var PACK_DIR = process.env.JSBIN;

//files
var CONFIG = './TScript/tsconfig.json';
var MEETING_JS = 'sfbmeeting.js';
var MEETING_JS_PACK = 'NLMeeting.js';

//glob
var ALL = '**';
var NOT = '!';
var SEP = '/';
var DECLARE_FILE = '*.d.ts';

gulp.task('default', function(){
    run('clean-output', 'compile-meeting', 'output-compiled-files', 'pack-meeting', 'output-packed-files');
});

gulp.task('clean-output', function () {
    return del([OUT_DIR+SEP+ALL, PACK_DIR+SEP+ALL, NOT+OUT_DIR, NOT+PACK_DIR], { force: true }).then(function (paths) {
        console.log('\nNLAssistantWebService typescript files deleted : \n' + paths.join('\n'));
    }).catch(function(err){
        console.log(`\n delete failed, err: ${err}`);
    });
});

gulp.task('compile-meeting', function () {
    var tsproj = gts.createProject(CONFIG); 
    return tsproj.src().pipe(tsproj()).pipe(gulp.dest(OUT_DIR));
});

gulp.task('pack-meeting', function () {
    return browserify().add(OUT_DIR+SEP+MEETING_JS).bundle().pipe(vss(MEETING_JS_PACK)).pipe(buffer()).pipe(gulp.dest(PACK_DIR));
});

gulp.task('output-compiled-files', function(){
    console.log('\nNLAssistantWebService typescript files compiled: ');
    var files = fs.readdirSync(OUT_DIR);
    files.forEach(function(file){
        console.log(`${path.join(__dirname, OUT_DIR, file)}`);
    });
});

gulp.task('output-packed-files', function(){
    console.log('\nNLAssistantWebService typescript files packed: ');

    var files = fs.readdirSync(PACK_DIR);
    files.forEach(function(file){
        console.log(`${path.join(__dirname, PACK_DIR, file)}`);
    });
});

gulp.start('default', function(err){
    if(err){
        console.log('\n');
        console.log(`NLAssistantWebService Task Failed, Error: ${err}`);
    }
    else{
        console.log('\n');
        console.log(`NLAssistantWebService Task Completed!`);
    }
});