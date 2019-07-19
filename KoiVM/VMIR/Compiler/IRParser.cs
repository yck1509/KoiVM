/*
 * IRParser.cs
 *
 * THIS FILE HAS BEEN GENERATED AUTOMATICALLY. DO NOT EDIT!
 */

using System.IO;

using PerCederberg.Grammatica.Runtime;

namespace KoiVM.VMIR.Compiler {

    /**
     * <remarks>A token stream parser.</remarks>
     */
    internal class IRParser : RecursiveDescentParser {

        /**
         * <summary>An enumeration with the generated production node
         * identity constants.</summary>
         */
        private enum SynteticPatterns {
            SUBPRODUCTION_1 = 3001,
            SUBPRODUCTION_2 = 3002,
            SUBPRODUCTION_3 = 3003,
            SUBPRODUCTION_4 = 3004,
            SUBPRODUCTION_5 = 3005,
            SUBPRODUCTION_6 = 3006,
            SUBPRODUCTION_7 = 3007,
            SUBPRODUCTION_8 = 3008,
            SUBPRODUCTION_9 = 3009,
            SUBPRODUCTION_10 = 3010,
            SUBPRODUCTION_11 = 3011,
            SUBPRODUCTION_12 = 3012
        }

        /**
         * <summary>Creates a new parser with a default analyzer.</summary>
         *
         * <param name='input'>the input stream to read from</param>
         *
         * <exception cref='ParserCreationException'>if the parser
         * couldn't be initialized correctly</exception>
         */
        public IRParser(TextReader input)
            : base(input) {

            CreatePatterns();
        }

        /**
         * <summary>Creates a new parser.</summary>
         *
         * <param name='input'>the input stream to read from</param>
         *
         * <param name='analyzer'>the analyzer to parse with</param>
         *
         * <exception cref='ParserCreationException'>if the parser
         * couldn't be initialized correctly</exception>
         */
        public IRParser(TextReader input, IRAnalyzer analyzer)
            : base(input, analyzer) {

            CreatePatterns();
        }

        /**
         * <summary>Creates a new tokenizer for this parser. Can be overridden
         * by a subclass to provide a custom implementation.</summary>
         *
         * <param name='input'>the input stream to read from</param>
         *
         * <returns>the tokenizer created</returns>
         *
         * <exception cref='ParserCreationException'>if the tokenizer
         * couldn't be initialized correctly</exception>
         */
        protected override Tokenizer NewTokenizer(TextReader input) {
            return new IRTokenizer(input);
        }

        /**
         * <summary>Initializes the parser by creating all the production
         * patterns.</summary>
         *
         * <exception cref='ParserCreationException'>if the parser
         * couldn't be initialized correctly</exception>
         */
        private void CreatePatterns() {
            ProductionPattern             pattern;
            ProductionPatternAlternative  alt;

            pattern = new ProductionPattern((int) IRConstants.PROGRAM,
                                            "Program");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.BLOCK, 1, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.BLOCK,
                                            "Block");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_1, 1, 1);
            alt.AddToken((int) IRConstants.NEWLINE, 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.DATA,
                                            "Data");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.DATA_DECL, 1, 1);
            alt.AddToken((int) IRConstants.ID, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_2, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.DATA_SIZE,
                                            "DataSize");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.NUM, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.DATA_CONTENT,
                                            "DataContent");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.EQUALS, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_3, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.DATA_STRING,
                                            "DataString");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.STR, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.DATA_BUFFER,
                                            "DataBuffer");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.LEFT_BRACKET, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_4, 0, -1);
            alt.AddToken((int) IRConstants.NUM, 1, 1);
            alt.AddToken((int) IRConstants.RIGHT_BRACKET, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.LEFT_BRACKET, 1, 1);
            alt.AddToken((int) IRConstants.RIGHT_BRACKET, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.CODE,
                                            "Code");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.CODE_HEADER, 1, 1);
            alt.AddToken((int) IRConstants.ID, 1, 1);
            alt.AddToken((int) IRConstants.NEWLINE, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_6, 0, -1);
            alt.AddToken((int) IRConstants.CODE_FOOTER, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.INSTR,
                                            "Instr");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.OP_CODE, 1, 1);
            alt.AddToken((int) IRConstants.NEWLINE, 1, -1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.OP_CODE, 1, 1);
            alt.AddProduction((int) IRConstants.OPERAND, 1, 1);
            alt.AddToken((int) IRConstants.NEWLINE, 1, -1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.OP_CODE, 1, 1);
            alt.AddProduction((int) IRConstants.OPERAND, 1, 1);
            alt.AddToken((int) IRConstants.COMMA, 1, 1);
            alt.AddProduction((int) IRConstants.OPERAND, 1, 1);
            alt.AddToken((int) IRConstants.NEWLINE, 1, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.OP_CODE,
                                            "OpCode");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.ID, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.OPERAND,
                                            "Operand");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.NUMBER, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.REGISTER, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.POINTER, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.LEFT_BRACE, 1, 1);
            alt.AddProduction((int) IRConstants.REFERENCE, 1, 1);
            alt.AddToken((int) IRConstants.RIGHT_BRACE, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.NUMBER,
                                            "Number");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_7, 0, 1);
            alt.AddToken((int) IRConstants.NUM, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.REGISTER,
                                            "Register");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_8, 0, 1);
            alt.AddToken((int) IRConstants.REG, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.POINTER,
                                            "Pointer");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_9, 0, 1);
            alt.AddToken((int) IRConstants.LEFT_BRACKET, 1, 1);
            alt.AddProduction((int) IRConstants.REGISTER, 1, 1);
            alt.AddProduction((int) IRConstants.POINTER_OFFSET, 0, 1);
            alt.AddToken((int) IRConstants.RIGHT_BRACKET, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.POINTER_OFFSET,
                                            "PointerOffset");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_10, 1, 1);
            alt.AddToken((int) IRConstants.NUM, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.REFERENCE,
                                            "Reference");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.ID, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.TYPE,
                                            "Type");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.ASTTYPE, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_11, 0, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.RAW_TYPE,
                                            "RawType");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.ID, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.TYPE_REF_DECL,
                                            "TypeRefDecl");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.TYPE_DECL, 1, 1);
            alt.AddToken((int) IRConstants.ID, 1, 1);
            alt.AddProduction((int) IRConstants.TYPE_REF, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.METHOD_REF_DECL,
                                            "MethodRefDecl");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.METHOD_DECL, 1, 1);
            alt.AddToken((int) IRConstants.ID, 1, 1);
            alt.AddProduction((int) IRConstants.METHOD_REF, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.TYPE_REF,
                                            "TypeRef");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.STR, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.METHOD_REF,
                                            "MethodRef");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.INSTANCE, 0, 1);
            alt.AddProduction((int) IRConstants.METHOD_RET_TYPE, 1, 1);
            alt.AddProduction((int) IRConstants.TYPE_REF, 1, 1);
            alt.AddToken((int) IRConstants.DELIM, 1, 1);
            alt.AddToken((int) IRConstants.STR, 1, 1);
            alt.AddProduction((int) IRConstants.METHOD_PARAMS, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.METHOD_RET_TYPE,
                                            "MethodRetType");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.TYPE_REF, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) IRConstants.METHOD_PARAMS,
                                            "MethodParams");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.LEFT_PAREN, 1, 1);
            alt.AddProduction((int) IRConstants.TYPE_REF, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_12, 0, -1);
            alt.AddToken((int) IRConstants.RIGHT_PAREN, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.LEFT_PAREN, 1, 1);
            alt.AddToken((int) IRConstants.RIGHT_PAREN, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_1,
                                            "Subproduction1");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.COMMENT, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.DATA, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.CODE, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.TYPE_REF_DECL, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.METHOD_REF_DECL, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_2,
                                            "Subproduction2");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.LEFT_PAREN, 1, 1);
            alt.AddProduction((int) IRConstants.DATA_SIZE, 1, 1);
            alt.AddToken((int) IRConstants.RIGHT_PAREN, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.DATA_CONTENT, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_3,
                                            "Subproduction3");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.DATA_BUFFER, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.DATA_STRING, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_4,
                                            "Subproduction4");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.NUM, 1, 1);
            alt.AddToken((int) IRConstants.COMMA, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_5,
                                            "Subproduction5");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.COMMENT, 1, 1);
            alt.AddToken((int) IRConstants.NEWLINE, 1, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_6,
                                            "Subproduction6");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_5, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.INSTR, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_7,
                                            "Subproduction7");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.TYPE, 1, 1);
            alt.AddToken((int) IRConstants.COLON, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_8,
                                            "Subproduction8");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.TYPE, 1, 1);
            alt.AddToken((int) IRConstants.COLON, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_9,
                                            "Subproduction9");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) IRConstants.TYPE, 1, 1);
            alt.AddToken((int) IRConstants.COLON, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_10,
                                            "Subproduction10");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.PLUS, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.MINUS, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_11,
                                            "Subproduction11");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.LEFT_PAREN, 1, 1);
            alt.AddProduction((int) IRConstants.RAW_TYPE, 1, 1);
            alt.AddToken((int) IRConstants.RIGHT_PAREN, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_12,
                                            "Subproduction12");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) IRConstants.COMMA, 1, 1);
            alt.AddProduction((int) IRConstants.TYPE_REF, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);
        }
    }
}
