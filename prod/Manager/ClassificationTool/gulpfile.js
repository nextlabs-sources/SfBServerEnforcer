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
var CLASSIFICATION_JS = 'classification.js';
var CLASSIFICATION_JS_PACK = 'NLClassification.js';
var SCHEMA_JS = 'schema.js';
var SCHEMA_JS_PACK = 'NLSchema.js';

//glob
var ALL = '**';
var NOT = '!';
var SEP = '/';
var DECLARE_FILE = '*.d.ts';

gulp.task('default', function(){
    run('clean-output', 'compile-all', 'output-compiled-files', 'pack-classification', 'pack-schema', 'output-packed-files');
});

gulp.task('clean-output', function () {
    return del([OUT_DIR+SEP+ALL, PACK_DIR+SEP+ALL, NOT+OUT_DIR, NOT+PACK_DIR], { force: true }).then(function (paths) {
        console.log('\nClassificationTool typescript files deleted : \n' + paths.join('\n'));
    }).catch(function(err){
        console.log(`\n delete failed, err: ${err}`);
    });
});

gulp.task('compile-all', function () {
    var tsproj = gts.createProject(CONFIG); 
    return tsproj.src().pipe(tsproj()).pipe(gulp.dest(OUT_DIR));
});

gulp.task('pack-classification', function () {
    return browserify().add(OUT_DIR+SEP+CLASSIFICATION_JS).bundle().pipe(vss(CLASSIFICATION_JS_PACK)).pipe(buffer()).pipe(gulp.dest(PACK_DIR));
});

gulp.task('pack-schema', function () {
    return browserify().add(OUT_DIR+SEP+SCHEMA_JS).bundle().pipe(vss(SCHEMA_JS_PACK)).pipe(buffer()).pipe(gulp.dest(PACK_DIR));
});

gulp.task('output-compiled-files', function(){
    console.log('\nClassificationTool typescript files compiled: ');
    var files = fs.readdirSync(OUT_DIR);
    files.forEach(function(file){
        console.log(`${path.join(__dirname, OUT_DIR, file)}`);
    });
});

gulp.task('output-packed-files', function(){
    console.log('\nClassificationTool typescript files packed: ');

    var files = fs.readdirSync(PACK_DIR);
    files.forEach(function(file){
        console.log(`${path.join(__dirname, PACK_DIR, file)}`);
    });
});

gulp.task('copy-pack-file', function(){
    gulp.src([PACK_DIR + SEP + ALL]).pipe(gulp.dest('./JScript'));
});

gulp.start('default', function(err){
    if(err){
        console.log('\n');
        console.log(`ClassificationTool Task Failed, Error: ${err}`);
    }
    else{
        console.log('\n');
        console.log(`ClassificationTool Task Completed!`);
    }
});