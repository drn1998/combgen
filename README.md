# combgen
combgen is a software tool to generate combinations from the source code of the eponymous scripting language.

As this is currently a work in progress, there is no formal specification or documentation available. If you would like to use combgen (at your own risk!) or learn more about it, I recommend studying the source code (including combgen's grammar, which is specified in ANTLR4) and to consult the example files which contain helpful comments.

## Use cases
Basically every situation where a lot of combinations of strings (and numbers) need to be generated. I especially think of:

- **Steganography and PIR:** When performing queries or asking questions, a lot of combinations can be generated. Among these, the actual question is hidden from many others that appear just as plausible. This use case, along with generating codebook entries, is the primary motivation behind the creation of combgen.
- **Research and scientific studies:** Systematic experiments where every combination is tested can be conducted by generating a list of all those combinations and performing them sequentially.
- **List of possible options:** To generate every possible option to consider them equally or rule out those which are not relevant or plausible.

combgen may also be applied to situations where not the enumeration of all the combinations, but just their number, is of question.

## Contribution

**This software is experimental and not ready for release! I welcome contribution (call +49 7022 5064970 or fax +49 7022 5064971 for any questions or coordination) however.**
