
SET GOAL=text
echo on
call  g:/bin/etc/params.%COMPUTERNAME%.cmd
echo off

SET BIB=g:\bin\bibDB


rem tex utility

SET LATEX=%P%\pdflatex.exe 
SET MKIDX=%P%\makeindex.exe 
SET BIBTEX=%P%\bibtex.exe 
SET EPS2PDF=%P%\epstopdf.exe

SET PAR=doxygen;class=article
SET PAR=doxygen;class=report

 
SET B=ascii
rem     my utility
rem graphViz utility


rem после отладныки документации новую библиографию сгрузить в %BIB%\*.bib
rem copy %BIB%\*.bib .


rm .%GOAL%.pdf
echo on
%MKT% %PAR%;preamble=12 <%GOAL%.txt > .%GOAL%.tex

%MKT% %PAR% <t_sum.r >.t_sum.tex
%MKT% %PAR% <t_dcl.r >.t_dcl.tex
%MKT% %PAR% <t_prg.r >.t_prg.tex
%MKT% %PAR% <t_gls.r >.t_gls.tex
%MKT% %PAR% <t_proj.r >.t_proj.tex
%MKT% %PAR% <t_yandex.r >.t_yandex.tex
%MKT% %PAR% <t_reference.r  >.t_reference.tex


%LATEX% -interaction=batchmode    .%GOAL%.tex


%MKIDX%  >.makeindex.log .%GOAL%.idx -o .%GOAL%.ind


%BIBTEX%  .%GOAL%   1>.%GOAL%.log

%LATEX% -interaction=batchmode   .%GOAL%.tex
%LATEX% -interaction=batchmode   .%GOAL%.tex
exit                            


