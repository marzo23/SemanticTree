using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Semantic_Tree
{
    class Program
    {
        static string letter = "[A-Za-z]";
        static string oper = @"[\>\&\|]";
        static string[] validOrder =
        {
            @"\![A-Za-z\(]", //empieza con negacion
            @"\([A-Za-z\(!]", //abrir parentesis
            @"[A-Za-z][\)\>\&\|]", //letra
            @"\)[\)\>\&\|]", //cerrar partentesis
            @"[\>\&\|][A-Za-z\(\!]" //operacion
        };
        static void Main(string[] args)
        {
            Console.WriteLine("ingrese expresión: ");
            string expresion = //"!(((p>q)>(!p|q))&((!p|q)>(p>q)))"; 
            "(!(((p>q)>(!p|q))&((!p|q)>(p>q))) & (((p>q)>(!p|q))&((!p|q)>(p>q))))";
            // Console.ReadLine();
            expresion = expresion.Trim().Replace(" ", "");
            List<Expression> expTmp = parseExpression(expresion);
            Expression t = getRootExpression(expTmp);
            List<CatExpression> cat = null;
            generateTable(t, null, ref cat);
            printCatalog(cat);
            //IEnumerable<CatExpression> cat1 = cat.Distinct();
            Console.ReadLine();
        }

        static void setSigns(CatExpression rootCat, Expression rootExp, ref CatExpression r)
        {
            //if (r == null)
        }

        static void printCatalog(List<CatExpression> cat)
        {
            Console.WriteLine("ID\tMax\tMin\tAtom");
            foreach (CatExpression c in cat)
            {
                Console.WriteLine(c.id + "\t" + (c.max.isPositive ? "" : "!") + c.max.id + "\t" + (c.min == null ? "0" : (c.min.isPositive ? "" : "!") + c.min.id) + "\t" + (c.atom != null ? c.atom.name : "-"));
            }
        }

        public static CatExpression find(List<CatExpression> t, CatExpression cE)
        {
            return t.Find(c =>
                    ((c.max != null && cE.max != null) ? (
                    c.max.id == cE.max.id &&
                    c.max.isPositive == cE.max.isPositive) : false) &&
                    ((c.min != null && cE.min != null) ? (
                    c.min.id == cE.min.id &&
                    c.min.isPositive == cE.min.isPositive) : false)
                    );
        }

        static void removeEquals(ref List<CatExpression> cat)
        {
            List<CatExpression> t = new List<CatExpression>();
            if (cat != null)
            {
                foreach (CatExpression cE in cat)
                {
                    CatExpression tmp = find(t, cE);
                    /*if (tmp != null && tmp != cE)
                    {
                        tmp.parentTmp = cE.parentTmp;
                        cE = tmp;
                    }*/
                    if (tmp == null)
                        t.Add(cE);
                    else if (tmp != cE)
                    {
                        List<CatExpression> toEval = new List<CatExpression>();
                        toEval.AddRange(cat);
                        List<CatExpression> evaluated = new List<CatExpression>();
                        while (toEval.Count > 0)
                        {
                            CatExpression current = toEval[0];
                            toEval.Remove(current);
                            evaluated.Add(current);
                            if (current.max != null)
                            {
                                CatExpression k = evaluated.Find(c => c == current || (
                                     ((c.max != null && current.max.max != null) ? (
                                     c.max.id == current.max.max.id &&
                                     c.max.isPositive == current.max.max.isPositive) : false) &&
                                     ((c.min != null && current.max.min != null) ? (
                                     c.min.id == current.max.min.id &&
                                     c.min.isPositive == current.max.min.isPositive) : false)
                                     ));
                                if (k == null)
                                    toEval.Add(current.max);
                                
                                    
                            }
                            if (current.min != null)
                            {
                                CatExpression k = evaluated.Find(c => c == current || (
                                     ((c.max != null && current.min.max != null) ? (
                                     c.max.id == current.min.max.id &&
                                     c.max.isPositive == current.min.max.isPositive) : false) &&
                                     ((c.min != null && current.min.min != null) ? (
                                     c.min.id == current.min.min.id &&
                                     c.min.isPositive == current.min.min.isPositive) : false)
                                    ));
                                if (k == null)
                                    toEval.Add(current.min);
                                                                  
                            }

                            if(current.id!=tmp.id && current.isPositive==tmp.isPositive 
                                && ((current.max != null && tmp.max != null) ? current.max.id==tmp.max.id && current.max.isPositive==tmp.max.isPositive : current.min == tmp.min)
                                && ((current.min!=null && tmp.min!=null)? current.min.id == tmp.min.id && current.min.isPositive == tmp.min.isPositive: current.min == tmp.min))
                            {
                                current = tmp;
                            }

                        }
                    }
                }
            }
            cat = t;
        }

        static void findAndReplace(ref Expression root, CatExpression target)
        {
            if (root == null)
                return;
            if(((root.tblExp.max!=null && target.max != null) ? root.tblExp.max.id==target.max.id && root.tblExp.max.isPositive == target.max.isPositive:false)&&
                ((root.tblExp.min != null && target.min != null) ? root.tblExp.min.id == target.min.id && root.tblExp.min.isPositive == target.min.isPositive : false))
            {
                root.tblExp = target;
            }
            //findAndReplace(ref root.e1, target);
        }

        static void generateTable(Expression root, CatExpression current, ref List<CatExpression> cat)
        {
            if (cat == null)
                cat = new List<CatExpression>();
            if(current==null)
                cat.Add(new CatExpression() { atom = null, id = cat.Count, max = null, min = null });
            int actualId = cat.Count - 1;

            if (root.a1 != null)
            {
                //*
                CatExpression tmp = cat.Find(c => c.atom!=null?(c.atom.name.Equals(root.a1.atom.name)):false && c.min == null && c.max!=null?(tmp.max.atom.id == tmp.max.atom.id):false);

                if (tmp == null)
                {
                    cat.Add(new CatExpression() { atom = root.a1.atom, id = cat.Count, min = null, isPositive = true });
                    tmp = cat.Last();
                }
                 //*/
                tmp = new CatExpression()
                {

                    atom = tmp.atom,
                    max = tmp,
                    min = null,
                    id = tmp.id,
                    isPositive = root.a1.isPositive
                };

                cat[actualId].max = tmp;
            }

            if (root.a2 != null)
            {
                //*
                CatExpression tmp = cat.Find(c => c.atom != null ? (c.atom.name.Equals(root.a2.atom.name)) : false && c.min == null && c.max != null ? (tmp.max.atom.id == tmp.max.atom.id) : false);
                if (tmp == null)
                {
                    cat.Add(new CatExpression() { atom = root.a2.atom, id = cat.Count, min = null, isPositive = true });
                    tmp = cat.Last();
                }
                    //*/
                 
                tmp = new CatExpression()
                {

                    atom = tmp.atom,
                    max = tmp,
                    min = null,
                    id = tmp.id,
                    isPositive = root.a2.isPositive
                };

                cat[actualId].min = tmp;
            }

            if (root.e1 != null)
            {
                int newId = cat.Count;
                
                cat.Add(new CatExpression() { atom = null, id = newId, max = null, min = null, isPositive = root.e1.isPositive });
                cat[actualId].max = cat.Last();
                generateTable(root.e1, cat.Last(), ref cat);
            }

            if (root.e2 != null)
            {
                int newId = cat.Count;
                cat.Add(new CatExpression() { atom = null, id = newId, max = null, min = null, isPositive= root.e2.isPositive });
                cat[actualId].min = cat.Last();
                generateTable(root.e2, cat.Last(), ref cat);
            }
            if (actualId == 0)
                Console.WriteLine("jdnc");
            
            
            if (cat[actualId].max != null && cat[actualId].min != null)
            {
                switch (root.operation)
                {
                    case 1:
                        cat[actualId].max.isPositive = false == cat[actualId].max.isPositive;
                        cat[actualId].min.isPositive = false == cat[actualId].min.isPositive;
                        break;
                    case 2:
                        cat[actualId].max.isPositive = cat[actualId].max.isPositive;
                        cat[actualId].min.isPositive = cat[actualId].min.isPositive;
                        break;
                    case 3:
                        cat[actualId].max.isPositive = false == cat[actualId].max.isPositive;
                        cat[actualId].min.isPositive = cat[actualId].min.isPositive;
                        break;
                }

                if (cat[actualId].max.id < cat[actualId].min.id)
                {
                    CatExpression tt = cat[actualId].max;
                    cat[actualId].max = cat[actualId].min;
                    cat[actualId].min = tt;
                }
                

                /*
                CatExpression cE = cat[actualId];
                CatExpression tmp = cat.Find(c => 
                c.max!=null?(
                c.max.id == cE.max.id && 
                c.max.isPositive == cE.max.isPositive):false &&
                c.min != null ? (
                c.min.id == cE.min.id && 
                c.min.isPositive == cE.min.isPositive) : false
                );
                if (tmp != null && tmp != cat[actualId])
                {
                    tmp.parentTmp = cat[actualId].parentTmp;
                    cat[actualId] = tmp;
                }
                //*/
            }
            CatExpression t = cat[actualId];
            CatExpression e = cat.Find(c => c != t && (
                                 ((c.max != null && t.max != null) ? (
                                 c.max.id == t.max.id &&
                                 c.max.isPositive == t.max.isPositive) : false) &&
                                 ((c.min != null && t.min != null) ? (
                                 c.min.id == t.min.id &&
                                 c.min.isPositive == t.min.isPositive) : false)
                                ));
            if (e != null)
            {
                if(e.isPositive == t.isPositive)
                    root.tblExp = e;
                else
                    root.tblExp = new CatExpression()
                    {
                        id = e.id,
                        isPositive = t.isPositive,
                        max = e.max,
                        min = e.min,
                        atom = e.atom
                    };
                replace(t, e, ref cat);
                cat.Remove(t);
            }
            else
                root.tblExp = t;
        }

        static void replace(CatExpression oldC, CatExpression newC, ref List<CatExpression> catL)
        {
            for (int i = 0; i < catL.Count; i++)
            {
                List<CatExpression> evaluated = new List<CatExpression>();
                List<CatExpression> toEvaluate = new List<CatExpression>();
                toEvaluate.Add(catL[i]);
                while (toEvaluate.Count > 0)
                {
                    CatExpression cat = toEvaluate[0];
                    toEvaluate.Remove(cat);
                    if (cat.min != null ? evaluated.Find(e => e == cat.min) == null : false)
                    {
                        if (cat.min.id == oldC.id)
                        {
                            if (cat.min.isPositive == newC.isPositive)
                                cat.min = newC;
                            else
                                cat.min = new CatExpression()
                                {
                                    id = newC.id,
                                    isPositive = cat.min.isPositive,
                                    max = newC.max,
                                    min = newC.min,
                                    atom = newC.atom
                                };
                        }
                        else
                            toEvaluate.Add(cat.min);
                    }
                    if ((cat.max != null ? evaluated.Find(e => e == cat.max) == null : false) && cat.max != cat)
                    {
                        if (cat.max.id == oldC.id)
                        {
                            if (cat.max.isPositive == newC.isPositive)
                                cat.max = newC;
                            else
                                cat.max = new CatExpression()
                                {
                                    id = newC.id,
                                    isPositive = cat.max.isPositive,
                                    max = newC.max,
                                    min = newC.min,
                                    atom = newC.atom
                                };
                        }
                        else
                            toEvaluate.Add(cat.max);
                    }
                    if (cat != oldC)
                        evaluated.Add(cat);
                }
            }
        }

        static List<Expression> parseExpression(string expresion)
        {

            List<Expression> expTmp = new List<Expression>();
            List<Expression> expHierarchi = new List<Expression>();

            bool currentNegative = false;
            bool isValid = false, isFirst = true;
            List<string> atoms = new List<string>();
            int tmpId = -1;
            for (int i = 0; i < expresion.Length; i++)
            {
                if (i < expresion.Length - 1)
                {
                    string substring = expresion.Substring(i, 2);
                    for (int j = 0; j < validOrder.Length; j++)
                    {
                        if (Regex.IsMatch(substring, validOrder[j]))
                            isValid = true;
                    }
                }

                if (!isValid)
                    break;

                if (expresion[i].ToString().Equals("!") && expresion[i + 1].ToString().Equals("("))
                {
                    expTmp.Add(new Expression() { id = expTmp.Count, isPositive = false });
                    expHierarchi.Add(expTmp.Last());
                    currentNegative = true;
                }
                else if (expresion[i].ToString().Equals("("))
                {
                    if (!currentNegative)
                    {
                        expTmp.Add(new Expression() { id = expTmp.Count, isPositive = true });
                        expHierarchi.Add(expTmp.Last());

                    }
                    else
                        currentNegative = false;

                    tmpId = expTmp.Count - 1;

                    if (isFirst && expHierarchi.Count - 2 >= 0)
                    {
                        expHierarchi[expHierarchi.Count - 2].e1 = expTmp[tmpId];
                    }
                    else if (!isFirst && expHierarchi.Count - 2 >= 0)
                    {
                        expHierarchi[expHierarchi.Count - 2].e2 = expTmp[tmpId];
                    }
                    isFirst = true;
                }
                else if (expresion[i].ToString().Equals(")"))
                {
                    expHierarchi.Remove(expHierarchi.Last());
                    isFirst = true;
                }
                else if (Regex.IsMatch(expresion[i].ToString(), oper))
                {
                    isFirst = false;
                    tmpId = expTmp.Count - 1;
                    if (expHierarchi.Count == 0)
                    {
                        expTmp.Add(new Expression() { id = expTmp.Count, isPositive = true });
                        List<Expression> tmp = new List<Expression>();
                        tmp.Add(expTmp.Last());
                        tmp.AddRange(expHierarchi);
                        expHierarchi = tmp;
                        tmpId = expTmp.Count - 1;
                        expTmp[tmpId].e1 = expTmp[expTmp.Count - 2];
                    }
                    tmpId = expTmp.Count - 1;
                    switch (expresion[i])
                    {
                        case '&':
                            expHierarchi.Last().operation = 1;
                            break;
                        case '|':
                            expHierarchi.Last().operation = 2;
                            break;
                        case '>':
                            expHierarchi.Last().operation = 3;
                            break;
                    }
                }
                else if (Regex.IsMatch(expresion[i].ToString(), letter))
                {
                    bool isPositive = i != 0 ? !expresion[i - 1].ToString().Equals("!") : true;
                    int atomId = atoms.IndexOf(expresion[i].ToString());
                    if (atomId == -1)
                    {
                        atoms.Add(expresion[i].ToString());
                        atomId = atoms.Count - 1;
                    }
                    tmpId = expTmp.Count - 1;
                    if (tmpId < 0)
                    {
                        expTmp.Add(new Expression() { id = expTmp.Count, isPositive = true });
                        expHierarchi.Add(expTmp.Last());
                    }

                    tmpId = expTmp.Count - 1;
                    if (isFirst)
                    {
                        expHierarchi.Last().a1 = new Atom() { atom = new atom() { id = atomId, name = expresion[i].ToString() }, isPositive = isPositive };
                    }
                    else
                    {
                        expHierarchi.Last().a2 = new Atom() { atom = new atom() { id = atomId, name = expresion[i].ToString() }, isPositive = isPositive };
                    }
                }
            }

            if (!isValid)
                Console.WriteLine("Invalid Expression");
            return expTmp;
        }


        static Expression getRootExpression(List<Expression> expTmp)
        {
            List<Expression> pureExps = new List<Expression>();
            List<Expression> pureNodes = new List<Expression>();
            List<Expression> mixedExps = new List<Expression>();
            foreach (Expression item in expTmp)
            {
                if (item.e1 != null && item.e2 != null && item.a1 == null && item.a2 == null)
                    pureExps.Add(item);
                else if (item.a1 != null && item.a2 != null && item.e1 == null && item.e2 == null)
                    pureNodes.Add(item);
                else if (
                    (item.a1 != null && item.e2 != null && item.a2 == null && item.e1 == null) ||
                    (item.a2 != null && item.e1 != null && item.a1 == null && item.e2 == null))
                    mixedExps.Add(item);
            }
            if (mixedExps.Count + pureExps.Count + pureNodes.Count != expTmp.Count)
                return null;

            foreach (Expression item in pureExps)
            {
                if (pureExps.Find(e => e.e1 == item || e.e2 == item) == null && mixedExps.Find(e => e.e1 == item || e.e2 == item) == null)
                    return item;
            }
            foreach (Expression item in mixedExps)
            {
                if (pureExps.Find(e => e.e1 == item || e.e2 == item) == null && mixedExps.Find(e => e.e1 == item || e.e2 == item) == null)
                    return item;
            }
            if (pureNodes.Count == 1 && expTmp.Count == 1)
                return pureNodes[0];
            return null;
        }
    }

    class CatExpression
    {
        //public int? parentTmp { get; set; } = null;
        public int id { get; set; }
        public bool isPositive { get; set; } = true;
        public CatExpression max { get; set; } = null;
        public CatExpression min { get; set; } = null;
        public atom atom { get; set; } = null;
        public CatExpression()
        {
            max = this;
        }
    }

    class Expression
    {
        public int id { get; set; }
        public bool isPositive { get; set; } = true;
        public int operation { get; set; } = -1;
        public Atom a1 { get; set; } = null;
        public Atom a2 { get; set; } = null;
        public Expression e1 { get; set; } = null;
        public Expression e2 { get; set; } = null;
        public CatExpression tblExp { get; set; } = null;

        public bool isClosed()
        {
            if (
                (e1 != null && e2 != null) ||
                (a1 != null && a2 != null) ||
                (e1 != null && a2 != null) ||
                (a1 != null && e2 != null)
                )
                return true;
            return false;
        }
        static public bool Equals(Expression e1, Expression e2)
        {
            if (e1 == null && e2 == null)
                return true;
            if (e1 == e2)
                return true;
            if (e1 != null && e2 != null)
                return e1.Equals(e2);
            return false;
        }
        public bool Equals(Expression e)
        {
            if (e != null)
            {
                if (e.operation == this.operation &&
                    Atom.Equals(this.a1, e.a1) &&
                    Atom.Equals(this.a2, e.a2) &&
                    Equals(this.e1, e.e1) &&
                    Equals(this.e2, e.e2)
                   )
                    return true;
            }
            return false;
        }
    }

    class Atom
    {
        public atom atom { get; set; } = null;
        public bool isPositive { get; set; } = true;
        public int id { get; set; }
        public bool Equals(Atom a)
        {
            if (a.atom != null && a.isPositive==this.isPositive)
            {
                if (a.atom.name == this.atom.name)
                    return true;
            }
            return false;
        }

        public static bool Equals(Atom a1, Atom a2)
        {
            if (a1 == a2)
                return true;
            if (a1 == null && a2 == null)
                return true;
            else if(a1!=null && a2!=null)
                return a1.Equals(a2);
            
            return false;
        }
    }

    class atom
    {
        public string name { get; set; }
        public int id { get; set; }
    }
}
