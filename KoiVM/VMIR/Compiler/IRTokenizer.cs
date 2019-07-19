/*
 * IRTokenizer.cs
 *
 * THIS FILE HAS BEEN GENERATED AUTOMATICALLY. DO NOT EDIT!
 */

using System.IO;

using PerCederberg.Grammatica.Runtime;

namespace KoiVM.VMIR.Compiler {

    /**
     * <remarks>A character stream tokenizer.</remarks>
     */
    internal class IRTokenizer : Tokenizer {

        /**
         * <summary>Creates a new tokenizer for the specified input
         * stream.</summary>
         *
         * <param name='input'>the input stream to read</param>
         *
         * <exception cref='ParserCreationException'>if the tokenizer
         * couldn't be initialized correctly</exception>
         */
        public IRTokenizer(TextReader input)
            : base(input, false) {

            CreatePatterns();
        }

        /**
         * <summary>Initializes the tokenizer by creating all the token
         * patterns.</summary>
         *
         * <exception cref='ParserCreationException'>if the tokenizer
         * couldn't be initialized correctly</exception>
         */
        private void CreatePatterns() {
            TokenPattern  pattern;

            pattern = new TokenPattern((int) IRConstants.STR,
                                       "STR",
                                       TokenPattern.PatternType.REGEXP,
                                       "(\"[^\"]*\"|'[^']*')");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.NUM,
                                       "NUM",
                                       TokenPattern.PatternType.REGEXP,
                                       "(0x[0-9a-fA-F]+|-?[0-9]+(.[0-9]+)?)");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.REG,
                                       "REG",
                                       TokenPattern.PatternType.REGEXP,
                                       "(R0|R1|R2|R3|R4|R5|R6|R7|BP|SP|IP|FL|K1|K2|M1|M2)");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.ASTTYPE,
                                       "ASTTYPE",
                                       TokenPattern.PatternType.REGEXP,
                                       "(I4|I8|R4|R8|O|Ptr|ByRef)");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.ID,
                                       "ID",
                                       TokenPattern.PatternType.REGEXP,
                                       "[a-zA-Z_][a-zA-Z0-9_]*");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.WHITESPACE,
                                       "WHITESPACE",
                                       TokenPattern.PatternType.REGEXP,
                                       "[ \\t]+");
            pattern.Ignore = true;
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.NEWLINE,
                                       "NEWLINE",
                                       TokenPattern.PatternType.REGEXP,
                                       "[\\r\\n]+");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.COMMENT,
                                       "COMMENT",
                                       TokenPattern.PatternType.REGEXP,
                                       "//[^\\r\\n]*");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.DATA_DECL,
                                       "DATA_DECL",
                                       TokenPattern.PatternType.STRING,
                                       ".data");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.TYPE_DECL,
                                       "TYPE_DECL",
                                       TokenPattern.PatternType.STRING,
                                       ".type");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.METHOD_DECL,
                                       "METHOD_DECL",
                                       TokenPattern.PatternType.STRING,
                                       ".method");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.INSTANCE,
                                       "INSTANCE",
                                       TokenPattern.PatternType.STRING,
                                       ".instance");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.CODE_HEADER,
                                       "CODE_HEADER",
                                       TokenPattern.PatternType.STRING,
                                       ".code");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.CODE_FOOTER,
                                       "CODE_FOOTER",
                                       TokenPattern.PatternType.STRING,
                                       ".end");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.LEFT_ANG,
                                       "LEFT_ANG",
                                       TokenPattern.PatternType.STRING,
                                       "<");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.RIGHT_ANG,
                                       "RIGHT_ANG",
                                       TokenPattern.PatternType.STRING,
                                       ">");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.LEFT_PAREN,
                                       "LEFT_PAREN",
                                       TokenPattern.PatternType.STRING,
                                       "(");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.RIGHT_PAREN,
                                       "RIGHT_PAREN",
                                       TokenPattern.PatternType.STRING,
                                       ")");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.LEFT_BRACE,
                                       "LEFT_BRACE",
                                       TokenPattern.PatternType.STRING,
                                       "{");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.RIGHT_BRACE,
                                       "RIGHT_BRACE",
                                       TokenPattern.PatternType.STRING,
                                       "}");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.LEFT_BRACKET,
                                       "LEFT_BRACKET",
                                       TokenPattern.PatternType.STRING,
                                       "[");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.RIGHT_BRACKET,
                                       "RIGHT_BRACKET",
                                       TokenPattern.PatternType.STRING,
                                       "]");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.EQUALS,
                                       "EQUALS",
                                       TokenPattern.PatternType.STRING,
                                       "=");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.COMMA,
                                       "COMMA",
                                       TokenPattern.PatternType.STRING,
                                       ",");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.COLON,
                                       "COLON",
                                       TokenPattern.PatternType.STRING,
                                       ":");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.PLUS,
                                       "PLUS",
                                       TokenPattern.PatternType.STRING,
                                       "+");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.MINUS,
                                       "MINUS",
                                       TokenPattern.PatternType.STRING,
                                       "-");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.DOT,
                                       "DOT",
                                       TokenPattern.PatternType.STRING,
                                       ".");
            AddPattern(pattern);

            pattern = new TokenPattern((int) IRConstants.DELIM,
                                       "DELIM",
                                       TokenPattern.PatternType.STRING,
                                       "::");
            AddPattern(pattern);
        }
    }
}
