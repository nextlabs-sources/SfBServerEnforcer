var gulp = require('gulp');
var del = require('del');
var gts = require('gulp-typescript');
var run = require('run-sequence');
var fs = require('fs');
var path = require('path');

//change cwd to the dir current file belongs to
process.chdir(__dirname);

//folders
var OUT_DIR = process.env.NLBUILDROOT+'/'+process.env.JSBIN+'/Modules';

//files
var CONFIG = './Modules/tsconfig.json';

//glob
var ALL = '**';
var NOT = '!';
var SEP = '/';
var DECLARE_FILE = '*.d.ts';

gulp.task('default', function(){
    run('clean-output', 'copy-declaration', 'compile-module', 'output-compiled-files');
});

gulp.task('clean-output', function () {
    return del([OUT_DIR + SEP + ALL, NOT + OUT_DIR], { force: true }).then(function (paths) {
        console.log('\nModule js files deleted : \n' + paths.join('\n'));
    }).catch(function(err){
        console.log(`\n delete failed, err: ${err}`);
    });
});

//copy delcaration files.
gulp.task('copy-declaration', function () {
    var sourceFile = `./Modules/**/${ DECLARE_FILE }`;
    return gulp.src([sourceFile]).pipe(gulp.dest(OUT_DIR));
});

//compile main
gulp.task('compile-module', function () {
    var tsproj = gts.createProject(CONFIG);
    return tsproj.src().pipe(tsproj()).pipe(gulp.dest(OUT_DIR));
});

gulp.task('output-compiled-files', function(){
    console.log('\nFrontEnd typescript files compiled: ');
    walk(OUT_DIR);
});

gulp.start('default', function(err){
    if(err){
        console.log('\n');
        console.log(`FrontEnd Task Failed, Error: ${err}`);
    }
    else{
        console.log('\n');
        console.log(`FrontEnd Task Completed!`);
    }
});

function walk(dirPath){
    var files = fs.readdirSync(dirPath);
    files.forEach(function(file){
        var absPath = path.join(dirPath, file);
        var tempStat = fs.statSync(absPath);
        if(tempStat.isDirectory()){
            walk(absPath);
        }
        else if(tempStat.isFile()){
            console.log(`${absPath}`);
        }
    });
}
